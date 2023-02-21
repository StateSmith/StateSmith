using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    internal static void VisitLeadingTriviaWith(this SyntaxNode node, CSharpSyntaxWalker walker)
    {
        node.GetLeadingTrivia().VisitWith(walker);
    }

    internal static void VisitTrailingTriviaWith(this SyntaxNode node, CSharpSyntaxWalker walker)
    {
        node.GetTrailingTrivia().VisitWith(walker);
    }

    internal static void VisitWith(this SyntaxTriviaList syntaxTrivias, CSharpSyntaxWalker walker)
    {
        walker.VisitTriviaList(syntaxTrivias);
    }

    internal static void VisitTriviaList(this CSharpSyntaxWalker walker, SyntaxTriviaList syntaxTrivias)
    {
        foreach (var trivia in syntaxTrivias)
        {
            walker.VisitTrivia(trivia);
        }
    }

    internal static bool IsPublic(this MethodDeclarationSyntax node)
    {
        return node.Modifiers.Any(d => (SyntaxKind)d.RawKind == SyntaxKind.PublicKeyword);
    }

    internal static void AppendLineIfNotBlank(this StringBuilder sb, string text)
    {
        if (text != string.Empty)
            sb.AppendLine(text);
    }
}
