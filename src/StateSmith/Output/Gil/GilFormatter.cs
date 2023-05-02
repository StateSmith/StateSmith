using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Input.Antlr4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateSmith.Output.Gil;

public class GilFormatter : CSharpSyntaxWalker
{
    public bool deIndentInnerClass = true;
    public bool deIndentMethods = true;
    public bool deIndentEnums = true;
    public bool deIndentDelegates = true;
    public bool deIndentConst = true;

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

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        MaybeDeIndent(deIndentInnerClass, node, () => base.VisitClassDeclaration(node));
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        MaybeDeIndent(deIndentMethods, node, () => base.VisitMethodDeclaration(node));
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        MaybeDeIndent(deIndentMethods, node, () => base.VisitConstructorDeclaration(node));
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        MaybeDeIndent(deIndentEnums, node, () => base.VisitEnumDeclaration(node));
    }

    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        MaybeDeIndent(deIndentDelegates, node, () => base.VisitDelegateDeclaration(node));
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        bool shouldDeIndent = deIndentConst && node.IsConst();
        MaybeDeIndent(shouldDeIndent, node, () => base.VisitFieldDeclaration(node));
    }

    private static SyntaxTrivia GetIndentationLevel(SyntaxNode node)
    {
        // will return None trivia on default
        return node.GetFirstToken().LeadingTrivia.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).LastOrDefault();
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
            sb.Append(toAppend);
        }
        else
        {
            var deIndent = deIndentStack.Peek();

            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                StringUtils.RemoveSpecificIndentSb(sb, toAppend, indent: deIndent);
            }
            else if (atStartOfLine && trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (toAppend.StartsWith(deIndent))
                    toAppend = toAppend.Substring(deIndent.Length);

                sb.Append(toAppend);
            }
            else
            {
                sb.Append(toAppend);
            }
            atStartOfLine = false;
        }

    }

    public void MaybeDeIndent(bool shouldDeIndent, SyntaxNode node, Action a)
    {
        if (shouldDeIndent)
        {
            var deIndent = GetIndentationLevel(node);
            deIndentStack.Push(deIndent.ToString());
        }

        a();

        if (shouldDeIndent)
        {
            deIndentStack.Pop();
        }
    }
}
