using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public static void AppendLineIfNotBlank(this StringBuilder sb, string text)
    {
        if (text != string.Empty)
            sb.AppendLine(text);
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
    /// TODO use <see cref="WalkableChildSyntaxList"/> instead. Remove this function.
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

    /// <summary>
    /// TODO use <see cref="WalkableChildSyntaxList"/> instead. Remove this function.
    /// </summary>
    public static void VisitChildNodesAndTokens(this SyntaxNode node, CSharpSyntaxWalker syntaxWalker, SyntaxToken token, Action action, SyntaxToken? toSkip = null)
    {
        var kids = node.ChildNodesAndTokens();
        int i = 0;

        while (true)
        {
            var kid = kids[i];
            i++;
            if (ShouldVisit(toSkip, kid))
                kid.VisitWith(syntaxWalker);

            if (kid == token)
                break;
        }

        action();

        for (; i < kids.Count; i++)
        {
            var kid = kids[i];
            if (ShouldVisit(toSkip, kid))
                kid.VisitWith(syntaxWalker);
        }

        static bool ShouldVisit(SyntaxToken? toSkip, SyntaxNodeOrToken kid)
        {
            return toSkip != kid;
        }
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
}
