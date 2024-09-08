#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Antlr4.Runtime.Misc;
using System.Net;

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

    public static GilTranspilerHelper Create(CSharpSyntaxWalker transpilerWalker, string gilCode, RoslynCompiler roslynCompiler)
    {
        roslynCompiler.Compile(gilCode, out var root, out var model);
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

    public bool HandleSpecialGilEmitClasses(ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (IsGilFileTopClass(classDeclarationSyntax))
        {
            return true;
        }

        return false;
    }

    public static bool IsGilFileTopClass(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.Identifier.Text == GilCreationHelper.GilClassName_FileTop;
    }

    /// <summary>
    /// Some gil expressions need to be handled specially to avoid outputting additional semicolons.
    /// </summary>
    /// <param name="expressionNode"></param>
    /// <param name="sb"></param>
    /// <returns></returns>
    public bool HandleGilSpecialExpressionStatements(ExpressionStatementSyntax expressionNode, StringBuilder sb)
    {
        bool gilEmitMethodFoundAndHandled = false;

        if (expressionNode.Expression is InvocationExpressionSyntax ies)
        {
            if (ies.Expression is IdentifierNameSyntax ins)
            {
                if (ins.Identifier.Text == GilCreationHelper.GilFuncName_EchoStringStatement)
                {
                    this.transpilerWalker.VisitLeadingTrivia(ies.GetFirstToken());
                    gilEmitMethodFoundAndHandled = true;
                    ProcessGilEchoInvocations(sb, ies);
                    this.transpilerWalker.VisitTrailingTrivia(expressionNode.GetLastToken());
                }
            }
        }

        return gilEmitMethodFoundAndHandled;
    }

    public bool HandleGilSpecialInvocations(InvocationExpressionSyntax node, StringBuilder sb)
    {
        bool gilEmitMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilCreationHelper.GilFuncName_EchoStringBool)
            {
                gilEmitMethodFoundAndHandled = true;
                ProcessGilEchoInvocations(sb, node);
            }
            else if (ins.Identifier.Text == GilCreationHelper.GilFuncName_VarArgsToBool)
            {
                gilEmitMethodFoundAndHandled = true;
                foreach (var arg in node.ArgumentList.Arguments)
                {
                    this.transpilerWalker.Visit(arg);
                }
            }
        }

        return gilEmitMethodFoundAndHandled;
    }

    /// <summary>
    /// The code to echo is wrapped in quotes (to make it a string) and then passed to this function.
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="ies"></param>
    private static void ProcessGilEchoInvocations(StringBuilder sb, InvocationExpressionSyntax ies)
    {
        ArgumentSyntax argumentSyntax = ies.ArgumentList.Arguments.Single();
        var unescaped = System.Text.RegularExpressions.Regex.Unescape(argumentSyntax.ToFullString());
        unescaped = unescaped[1..^1]; // remove surrounding quotes present because of gil wrapping
        sb.Append(unescaped); // FIXME: this may not do everything we need. We need inverse of https://stackoverflow.com/a/58825732/7331858 
    }

    public bool HandleGilUnusedVarSpecialInvocation(InvocationExpressionSyntax node, Action<ArgumentSyntax> codeBuilder)
    {
        bool gilMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilCreationHelper.GilFuncName_UnusedVar)
            {
                gilMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                codeBuilder(argumentSyntax);
            }
        }

        return gilMethodFoundAndHandled;
    }

    public string GetFQN(ISymbol symbol)
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
}
