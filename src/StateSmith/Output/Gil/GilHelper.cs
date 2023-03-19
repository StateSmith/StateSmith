#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using StateSmith.Common;
using System;

namespace StateSmith.Output.Gil;

public class GilHelper
{
    protected const string GilNoEmitPrefix = "____GilNoEmit_";
    protected const string GilNoEmitEchoStringBoolFuncName = GilNoEmitPrefix + "echoStringBool";
    protected const string GilUnusedVarFuncName = GilNoEmitPrefix + "GilUnusedVar";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public static bool {GilNoEmitEchoStringBoolFuncName}(string toEcho) {{ return true; }}");
        file.AppendLine($"public static void {GilUnusedVarFuncName}(object unusedVar) {{ }}");
    }

    public static string MarkVarAsUnused(string varName)
    {
        return $"{GilUnusedVarFuncName}({varName});";
    }

    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        codeToWrap = SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
        return $"{GilNoEmitEchoStringBoolFuncName}({codeToWrap})";
    }

    public static bool IsGilNoEmit(string identifierName)
    {
        return identifierName.StartsWith(GilNoEmitPrefix);
    }

    public static bool IsGilNoEmit(MethodDeclarationSyntax node)
    {
        return IsGilNoEmit(node.Identifier.ValueText);
    }

    public static void Compile(string programText, out CompilationUnitSyntax root, out SemanticModel model)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
        root = tree.GetCompilationUnitRoot();
        ThrowOnError(tree.GetDiagnostics(), programText);

        var compilation = CSharpCompilation.Create("GilCompilation")
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        model = compilation.GetSemanticModel(tree);
        ThrowOnError(model.GetDiagnostics(), programText);
    }

    public record AddressableFunctionInfo(INamedTypeSymbol DelegateSymbol, ParameterListSyntax ParameterListSyntax);

    public static bool HandleSpecialGilEmitClasses(ClassDeclarationSyntax classDeclarationSyntax, CSharpSyntaxWalker walker)
    {
        // remove method if not used in an a while
        return false;
    }

    public static bool HandleGilSpecialInvocations(InvocationExpressionSyntax node, StringBuilder sb)
    {
        bool gilEmitMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilHelper.GilNoEmitEchoStringBoolFuncName)
            {
                gilEmitMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(argumentSyntax.ToFullString());
                unescaped = unescaped[1..^1]; // range operator
                sb.Append(unescaped); // FIXME: this may not do everything we need. We need inverse of https://stackoverflow.com/a/58825732/7331858 
            }
        }

        return gilEmitMethodFoundAndHandled;
    }

    public static bool HandleGilUnusedVarSpecialInvocation(InvocationExpressionSyntax node, Action<ArgumentSyntax> codeBuilder)
    {
        bool gilMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilHelper.GilUnusedVarFuncName)
            {
                gilMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                codeBuilder(argumentSyntax);
            }
        }

        return gilMethodFoundAndHandled;
    }

    private static void ThrowOnError(IEnumerable<Diagnostic> enumerable, string programText)
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

    public static string GetFQN(ISymbol symbol)
    {
        var parts = new List<string>();

        parts.Insert(index: 0, symbol.Name);
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
                parts.Insert(index: 0, symbol.Name);

            symbol = symbol.ContainingSymbol;
        }

        var fqn = string.Join(".", parts);
        return fqn;
    }

    public static bool ExpressionIsEnumMember(SemanticModel model, ExpressionSyntax expressionSyntax)
    {
        ISymbol? symbol = model.GetSymbolInfo(expressionSyntax).Symbol;
        return symbol.IsEnumMember();
    }

    public static bool IsEnumMemberConversionToInt(SemanticModel model, CastExpressionSyntax node)
    {
        if (node.Type is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.IntKeyword))
        {
            if (GilHelper.ExpressionIsEnumMember(model, node.Expression))
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
    /// <param name="model"></param>
    /// <returns></returns>
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
    /// <param name="model"></param>
    /// <param name="walker"></param>
    /// <returns></returns>
    public static bool HandleThisMethodAccess(MemberAccessExpressionSyntax node, SemanticModel model, CSharpSyntaxWalker walker)
    {
        if (IsThisMethodAccess(node, model))
        {
            walker.VisitLeadingTrivia(node.GetFirstToken());
            node.Name.VisitWith(walker);
            return true;
        }

        return false;
    }
}
