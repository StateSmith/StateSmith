using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateSmith.Output.Gil;

public class GilFormatter : CSharpSyntaxWalker
{
    public bool deIndentInnerClass = true;
    public bool deIndentMethods = true;
    public StringBuilder sb = new();
    private readonly Stack<string> deIndentStack = new();
    private bool atStartOfLine = false;

    public GilFormatter() : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        deIndentStack.Push("");
    }

    public static string Format(string input)
    {
        var root = CSharpSyntaxTree.ParseText(input).GetCompilationUnitRoot();
        var formatter = new GilFormatter();
        formatter.Visit(root);
        return formatter.sb.ToString();
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax cls)
    {
        var tokensAndKids = new WalkableChildSyntaxList(this, cls);
        tokensAndKids.VisitUpTo(cls.OpenBraceToken, including: true);

        var indentationAfterOpeningBrace = cls.OpenBraceToken.GetNextToken().LeadingTrivia.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).LastOrDefault(); // will return None trivia on default
        deIndentStack.Push(indentationAfterOpeningBrace.ToString());

        tokensAndKids.VisitUpTo(cls.CloseBraceToken, including: false);
        deIndentStack.Pop();
        VisitToken(cls.CloseBraceToken);
    }

    public override void VisitToken(SyntaxToken token)
    {
        VisitLeadingTrivia(token);
        sb.Append(token.ToString());
        atStartOfLine = false; // We just output a token. can't be start of a line.
        VisitTrailingTrivia(token);
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        string toAppend = trivia.ToString();

        if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
        {
            atStartOfLine = true;
        }
        else
        {
            if (atStartOfLine && trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                var deIndent = deIndentStack.Peek();
                if (toAppend.StartsWith(deIndent))
                    toAppend = toAppend.Substring(deIndent.Length);
            }
            atStartOfLine = false;
        }

        sb.Append(toAppend);
    }
}
