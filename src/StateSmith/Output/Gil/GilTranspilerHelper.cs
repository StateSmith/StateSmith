#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace StateSmith.Output.Gil;

public class GilTranspilerHelper
{
    private readonly CSharpSyntaxWalker transpilerWalker;
    public readonly SemanticModel model;
    public readonly CompilationUnitSyntax root;

    public GilTranspilerHelper(CSharpSyntaxWalker transpilerWalker, SemanticModel model, CompilationUnitSyntax? root = null)
    {
        this.transpilerWalker = transpilerWalker;
        this.model = model;
        this.root = root ?? model.SyntaxTree.GetCompilationUnitRoot();
    }

    public static GilTranspilerHelper Create(CSharpSyntaxWalker transpilerWalker, string gilCode)
    {
        Compile(gilCode, out var root, out var model);
        return new GilTranspilerHelper(transpilerWalker, model, root);
    }

    public void PreProcess()
    {
        var gilFileTopClasses = root.ChildNodes().OfType<ClassDeclarationSyntax>().Where(cds => IsGilFileTopClass(cds));

        foreach (var node in gilFileTopClasses)
        {
            node.GetLeadingTrivia().VisitWith(transpilerWalker);
        }
    }

    public bool IsGilNoEmit(string identifierName)
    {
        return identifierName.StartsWith(GilCreationHelper.GilPrefix);
    }

    public bool IsGilNoEmit(MethodDeclarationSyntax node)
    {
        return IsGilNoEmit(node.Identifier.ValueText);
    }

    /// <summary>
    /// You'll probably want to get the compilation unit root with `tree.GetCompilationUnitRoot()`.
    /// </summary>
    /// <param name="gilCode"></param>
    /// <returns></returns>
    public static SyntaxTree ParseToSyntaxTree(string gilCode)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(gilCode);
        ThrowAnyDiagnosticError(tree.GetDiagnostics(), gilCode);
        return tree;
    }

    public static void Compile(string gilCode, out CompilationUnitSyntax root, out SemanticModel model)
    {
        SyntaxTree tree = ParseToSyntaxTree(gilCode);
        root = tree.GetCompilationUnitRoot();
        ThrowAnyDiagnosticError(tree.GetDiagnostics(), gilCode);

        var compilation = CSharpCompilation.Create("GilCompilation")
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        model = compilation.GetSemanticModel(tree);
        ThrowAnyDiagnosticError(model.GetDiagnostics(), gilCode);
    }

    internal static void ThrowAnyDiagnosticError(IEnumerable<Diagnostic> enumerable, string programText)
    {
        var errors = enumerable.Where(d => d.Severity == DiagnosticSeverity.Error);

        var message = "";

        foreach (var error in errors)
        {
            message += error.ToString() + "\n";
        }

        if (message.Length > 0)
        {
            throw new TranspilerException(message, programText);
        }
    }

    public bool HandleSpecialGilClasses(ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (IsGilFileTopClass(classDeclarationSyntax))
        {
            return true;
        }

        return false;
    }

    public static bool IsGilFileTopClass(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return GilCreationHelper.IsFileTopCommentClass(classDeclarationSyntax.Identifier.Text);
    }

    public bool HandleGilSpecialInvocations(InvocationExpressionSyntax node, StringBuilder sb)
    {
        bool gilMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilCreationHelper.GilEchoStringBoolReturnFuncName)
            {
                gilMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(argumentSyntax.ToFullString());
                unescaped = unescaped[1..^1]; // range operator
                sb.Append(unescaped); // FIXME: this may not do everything we need. We need inverse of https://stackoverflow.com/a/58825732/7331858 
            }
            else if (ins.Identifier.Text == GilCreationHelper.GilVisitVarArgsBoolReturnFuncName)
            {
                gilMethodFoundAndHandled = true;
                foreach (var arg in node.ArgumentList.Arguments)
                {
                    this.transpilerWalker.Visit(arg);
                }
            }
        }

        return gilMethodFoundAndHandled;
    }

    public bool HandleGilUnusedVarSpecialInvocation(InvocationExpressionSyntax node, Action<ArgumentSyntax> codeBuilder)
    {
        bool gilMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilCreationHelper.GilUnusedVarFuncName)
            {
                gilMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                codeBuilder(argumentSyntax);
            }
        }

        return gilMethodFoundAndHandled;
    }

    private static string GetName(ISymbol symbol)
    {
        if (symbol is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Constructor)
        {
            return "ctor";
        }
        return symbol.Name;
    }

    public string GetFQN(ISymbol symbol)
    {
        var parts = new List<string>();

        parts.Insert(index: 0, GetName(symbol));
        symbol = symbol.ContainingSymbol;

        while (symbol != null)
        {
            if (symbol is INamespaceSymbol namespaceSymbol)
            {
                // need to stop at global namespace to prevent ascending into dll
                if (namespaceSymbol.IsGlobalNamespace)
                {
                    break;
                }
            }

            if (symbol is not IMethodSymbol)
                parts.Insert(index: 0, GetName(symbol));

            symbol = symbol.ContainingSymbol;
        }

        var fqn = string.Join(".", parts);
        return fqn;
    }

    public bool ExpressionIsEnumMember(ExpressionSyntax expressionSyntax)
    {
        ISymbol? symbol = model.GetSymbolInfo(expressionSyntax).Symbol;
        return symbol.IsEnumMember();
    }

    public bool IsEnumMemberConversionToInt(CastExpressionSyntax node)
    {
        if (node.Type is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.IntKeyword))
        {
            if (ExpressionIsEnumMember(node.Expression))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// returns true if `this.SomeMethod`
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool IsThisMethodAccess(MemberAccessExpressionSyntax node)
    {
        var m = model;

        return IsThisMethodAccess(node, m);
    }

    public static bool IsThisMethodAccess(MemberAccessExpressionSyntax node, SemanticModel model)
    {
        if (node.IsThisMemberAccess())
        {
            if (node.Name.IsIMethodSymbol(model))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// `this.SomeMethod` to `SomeMethod`
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool HandleThisMethodAccess(MemberAccessExpressionSyntax node)
    {
        if (IsThisMethodAccess(node))
        {
            transpilerWalker.VisitLeadingTrivia(node.GetFirstToken());
            node.Name.VisitWith(transpilerWalker);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns attached comment trivia and possible trailing end of line trivia.
    /// Unattached comments have a full blank line trailing them:
    /// <code>
    /// // unattached comment
    /// 
    /// // attached comment
    /// void some_function()
    /// </code>
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static List<SyntaxTrivia> GetAttachedCommentTrivia(SyntaxNode node)
    {
        // Only output attached comments. If we find 2 or more end of line trivia without a comment trivia,
        // clear any stored trivia.
        List<SyntaxTrivia> toOutput = new();

        int endOfLineCount = 0;
        foreach (var t in node.GetLeadingTrivia())
        {
            bool isComment = t.IsKind(SyntaxKind.SingleLineCommentTrivia)
                          || t.IsKind(SyntaxKind.MultiLineCommentTrivia); // can also look at others like SingleLineDocumentationCommentTrivia

            if (t.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;
                if (endOfLineCount > 1)
                    toOutput.Clear();
                else if (toOutput.Any()) // append end of line if we already had a comment stored
                    toOutput.Add(t);
            }
            else if (isComment)
            {
                endOfLineCount = 0;
                toOutput.Add(t);
            }
            else
            {
                toOutput.Add(t);
            }
        }

        return toOutput;
    }
}
