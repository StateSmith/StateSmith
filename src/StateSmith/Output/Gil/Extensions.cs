using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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

    public static void VisitWith(this SyntaxNodeOrToken kid, CSharpSyntaxWalker walker)
    {
        if (kid.IsNode)
            walker.Visit(kid.AsNode());
        else
            walker.VisitToken(kid.AsToken());
    }

    // this should probably be moved out of extensions
    public static void VisitNodeRunActionAfterToken(this CSharpSyntaxWalker syntaxWalker, ClassDeclarationSyntax node, SyntaxToken token, Action action)
    {
        var kids = node.ChildNodesAndTokens();
        int i = 0;

        while (true)
        {
            var kid = kids[i];
            i++;
            kid.VisitWith(syntaxWalker);

            if (kid == token)
                break;
        }

        action();

        for (; i < kids.Count; i++)
        {
            var kid = kids[i];
            kid.VisitWith(syntaxWalker);
        }
    }
}
