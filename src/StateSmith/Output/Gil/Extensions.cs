using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Input.Antlr4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

#nullable enable

namespace StateSmith.Output.Gil;

public static class Extensions
{
    public static void VisitChildrenNodesWithWalker(this SyntaxNode node, CSharpSyntaxWalker walker)
    {
        foreach (var kid in node.ChildNodes())
        {
            walker.Visit(kid);
        }
    }

    public static void VisitLeadingTriviaWith(this SyntaxNode node, CSharpSyntaxWalker walker)
    {
        node.GetLeadingTrivia().VisitWith(walker);
    }

    public static void VisitTrailingTriviaWith(this SyntaxNode node, CSharpSyntaxWalker walker)
    {
        node.GetTrailingTrivia().VisitWith(walker);
    }

    public static void VisitWith(this SyntaxTriviaList syntaxTrivias, CSharpSyntaxWalker walker)
    {
        walker.VisitTriviaList(syntaxTrivias);
    }

    public static void VisitTriviaList(this CSharpSyntaxWalker walker, SyntaxTriviaList syntaxTrivias)
    {
        foreach (var trivia in syntaxTrivias)
        {
            walker.VisitTrivia(trivia);
        }
    }

    public static bool IsPublic(this MethodDeclarationSyntax node)
    {
        return node.Modifiers.Any(d => (SyntaxKind)d.RawKind == SyntaxKind.PublicKeyword);
    }

    public static void AppendLineIfNotBlank(this StringBuilder sb, string text, string optionalTrailer = "")
    {
        if (text != string.Empty)
        {
            sb.AppendLine(text);
            sb.AppendIfNotBlank(optionalTrailer);
        }
    }

    public static void AppendIfNotBlank(this StringBuilder sb, string text)
    {
        if (text != string.Empty)
            sb.Append(text);
    }

    public static void AppendTokenAndTrivia(this StringBuilder sb, SyntaxToken token, string? overrideTokenText = null)
    {
        sb.Append(token.LeadingTrivia);
        sb.Append(overrideTokenText ?? token.Text);
        sb.Append(token.TrailingTrivia);
    }

    public static void VisitWith(this SyntaxNodeOrToken kid, CSharpSyntaxWalker walker)
    {
        if (kid.IsNode)
            walker.Visit(kid.AsNode());
        else
            walker.VisitToken(kid.AsToken());
    }

    /// <summary>
    /// If you have more advanced needs, see <see cref="WalkableChildSyntaxList"/>.
    /// </summary>
    public static void VisitChildNodesAndTokens(this SyntaxNode node, CSharpSyntaxWalker syntaxWalker, SyntaxToken? toSkip = null)
    {
        var kids = node.ChildNodesAndTokens();

        foreach (var kid in kids)
        {
            if (kid != toSkip)
                kid.VisitWith(syntaxWalker);
        }
    }

    public static void VisitWith(this SyntaxNode node, CSharpSyntaxWalker syntaxWalker)
    {
        syntaxWalker.Visit(node);
    }

    public static void VisitWith(this SyntaxToken token, CSharpSyntaxWalker syntaxWalker)
    {
        syntaxWalker.VisitToken(token);
    }

    public static bool HasModifier(this SyntaxTokenList syntaxTokens, SyntaxKind syntaxKind)
    {
        return syntaxTokens.Any(d => (SyntaxKind)d.RawKind == syntaxKind);
    }

    public static bool IsConst(this FieldDeclarationSyntax? node)
    {
        if (node == null) return false;
        return node.Modifiers.HasModifier(SyntaxKind.ConstKeyword);
    }

    public static bool IsStatic(this FieldDeclarationSyntax? node)
    {
        if (node == null) return false;
        return node.Modifiers.HasModifier(SyntaxKind.StaticKeyword);
    }

    public static bool IsReadonly(this FieldDeclarationSyntax? node)
    {
        if (node == null) return false;
        return node.Modifiers.HasModifier(SyntaxKind.ReadOnlyKeyword);
    }

    public static bool IsEnumMember(this ISymbol? symbol)
    {
        if (symbol == null)
            return false;

        if (symbol is IParameterSymbol ps && ps.Type.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        if (symbol is IFieldSymbol f && f.Type.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        if (symbol is INamedTypeSymbol nts && nts.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        return false;
    }

    public static bool IsThisMemberAccess(this ExpressionSyntax node)
    {
        return node.IsThisMemberAccess(out _);
    }

    public static bool IsThisMemberAccess(this ExpressionSyntax node, out MemberAccessExpressionSyntax? memberAccessExpressionSyntax)
    {
        if (node is MemberAccessExpressionSyntax memberAccess && memberAccess.IsSimpleMemberAccess())
        {
            if (memberAccess.Expression is ThisExpressionSyntax)
            {
                memberAccessExpressionSyntax = memberAccess;
                return true;
            }
        }

        memberAccessExpressionSyntax = null;
        return false;
    }

    public static bool IsSimpleMemberAccess(this MemberAccessExpressionSyntax node)
    {
        return node.IsKind(SyntaxKind.SimpleMemberAccessExpression);
    }

    public static bool IsIMethodSymbol(this SimpleNameSyntax simpleNameSyntax, SemanticModel semanticModel)
    {
        return simpleNameSyntax.IsIMethodSymbol(semanticModel, out _);
    }

    public static bool IsIMethodSymbol(this SimpleNameSyntax simpleNameSyntax, SemanticModel semanticModel, out IMethodSymbol? methodSymbol)
    {
        if (semanticModel.GetSymbolInfo(simpleNameSyntax).Symbol is IMethodSymbol ims)
        {
            methodSymbol = ims;
            return true;
        }
        else
        {
            methodSymbol = null;
            return false;
        }
    }

}
