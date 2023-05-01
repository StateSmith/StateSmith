using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Input.Antlr4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateSmith.Output.Gil;

public class GilFormatter : CSharpSyntaxWalker
{
    public bool deIndentInnerClass = true;
    public bool deIndentMethods = true;
    public StringBuilder sb = new();
    Stack<string> deIndentStack = new();
    bool atStartOfLine = false;

    public GilFormatter() : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        deIndentStack.Push("");
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
