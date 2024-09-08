#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StateSmith.Output.Gil.Python;

/// <summary>
/// NOTE! This is mostly a copy of CSharp transpiler with some Python specific changes.
/// It still needs some updating: https://github.com/StateSmith/StateSmith/issues/398
/// </summary>
public class PythonGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder sb;
    private readonly RenderConfigPythonVars renderConfigPython;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly CodeStyleSettings codeStyleSettings;
    private Stack<string> classIndentStack = new();
    private bool prependSelf = false;

    private SemanticModel model;
    private string Indent => codeStyleSettings.Indent1;

    private SyntaxToken? tokenToSkip;

    public PythonGilVisitor(string gilCode, StringBuilder sb, RenderConfigPythonVars renderConfigPython, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler, CodeStyleSettings codeStyleSettings) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigPython = renderConfigPython;
        this.codeStyleSettings = codeStyleSettings;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, roslynCompiler);
        model = transpilerHelper.model;
    }

    public void Process()
    {
        transpilerHelper.PreProcess();

        sb.AppendLineIfNotBlank(renderConfig.FileTop);

        sb.AppendLine("import enum");

        sb.AppendLineIfNotBlank(renderConfigPython.Imports, optionalTrailer: "\n");

        this.Visit(transpilerHelper.root);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;
        PushClassIndent(node);

        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("class ");
        sb.Append(node.Identifier.Text);

        sb.Append($"({renderConfigPython.Extends.Trim()}):");
        VisitTrailingTrivia(node.Identifier);

        sb.AppendLineIfNotBlank(renderConfigPython.ClassCode);

        // if class doesn't have a constructor, add a default one
        if (!node.ChildNodes().OfType<ConstructorDeclarationSyntax>().Any())
        {
            AddIndent(1);
            sb.AppendLine("def __init__(self):");

            foreach (var member in node.Members)
            {
                if (member is FieldDeclarationSyntax)
                {
                    Visit(member);
                }
            }

            AddIndent(2);
            sb.AppendLine("pass"); // TODO - remove this when we have a better way to handle empty constructors
        }

        foreach (var member in node.Members)
        {
            if (member is not FieldDeclarationSyntax)
            {
                Visit(member);
            }
        }

        classIndentStack.Pop();
    }

    private void PushClassIndent(ClassDeclarationSyntax node)
    {
        string code = node.GetFirstToken().LeadingTrivia.ToFullString();

        var regex = new Regex(@"(?m)^([ \t]+)\z"); // \z is end of string

        var indent = regex.Match(code).Groups[1].Value.ThrowIfNull();
        classIndentStack.Push(indent);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("class ");
        sb.Append(node.Identifier.Text);
        sb.AppendLine("(enum.Enum):");

        foreach (var member in node.Members)
        {
            Visit(member);
            sb.AppendLine();
        }
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        // no open brace
        sb.Append("if ");
        Visit(node.Condition);
        sb.AppendLine(":");
        Visit(node.Statement);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        list.VisitUpTo(node.CloseParenToken, including: false); // need to not output trailing whitespace
        sb.Append(node.CloseParenToken.Text);
        sb.AppendLine(":");

        Visit(node.Statement);
    }

    public override void VisitBlock(BlockSyntax node)
    {
        sb.AppendLine();
        foreach (var item in node.Statements)
        {
            Visit(item);
        }
    }


    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append(Indent);
        sb.Append("self.");
        sb.Append(node.Declaration.Variables.First().Identifier.Text);
        sb.AppendLine(" = None");
    }

    private void AddIndent(int amount)
    {
        sb.Append(classIndentStack.Peek());
        for (int j = 0; j < amount; j++)
            sb.Append(Indent);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());

        if (node.ParameterList.ChildNodes().Any())
        {
            throw new NotImplementedException("Constructors with parameters are not yet supported.");
        }

        //AddIndent(1);
        sb.AppendLine("def __init__(self):");

        // get class fields
        var fields = node.Ancestors().OfType<ClassDeclarationSyntax>().First().ChildNodes().OfType<FieldDeclarationSyntax>();

        foreach (var field in fields)
        {
            Visit(field);
        }

        foreach (var statement in node.Body!.Statements)
        {
            Visit(statement);
        }
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var result = node.Identifier.Text;

        switch (result)
        {
            default:
                {
                    SymbolInfo symbol = model.GetSymbolInfo(node);
                    result = GetPythonName(symbol.Symbol.ThrowIfNull());
                    break;
                }
        }

        node.VisitLeadingTriviaWith(this);
        sb.Append(result);
        node.VisitTrailingTriviaWith(this);
    }

    private string GetPythonName(ISymbol symbol)
    {
        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (!fieldSymbol.IsStatic && !fieldSymbol.IsConst)
            {
                var result = "";
                if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
                    result = "_";

                result += fieldSymbol.Name;
                return result;
            }

            if (fieldSymbol.IsEnumMember())
            {
                return fieldSymbol.Name;
            }
        }

        if (symbol.Kind == SymbolKind.Parameter || symbol.Kind == SymbolKind.Local)
        {
            return symbol.Name;
        }

        if (symbol is IMethodSymbol methodSymbol)
        {
            var result = "";
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                result = "_";

            result += methodSymbol.Name;
            return result;
        }

        //var fqn = transpilerHelper.GetFQN(symbol);
        //return fqn;

        return symbol.Name;
    }

    public override void VisitNullableType(NullableTypeSyntax node)
    {
        Visit(node.ElementType); // this avoids outputting the `?` for a nullable type
        sb.Append(' ');
    }

    public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        // Fix for https://github.com/StateSmith/StateSmith/issues/231
        if (node.IsKind(SyntaxKind.SuppressNullableWarningExpression))
        {
            Visit(node.Operand);
        }
        else
        {
            base.VisitPostfixUnaryExpression(node);
        }
    }

    //public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    //{
    //    bool done = false;

    //    if (transpilerHelper.HandleThisMethodAccess(node))
    //        done = true;

    //    if (!done)
    //        base.VisitMemberAccessExpression(node);
    //}

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilNoEmit(node))
            return;

        bool isStatic = node.Modifiers.Any(SyntaxKind.StaticKeyword);

        if (isStatic)
        {
            sb.AppendLine("@staticmethod");
        }

        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("def ");

        //if private, add a leading underscore
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("_");
        }

        sb.Append(node.Identifier.Text);

        if (!isStatic)
        {
            prependSelf = true;
        }

        VisitParameterList(node.ParameterList);
        sb.Append(':');

        VisitBlock(node.Body!);
    }

    public override void VisitParameterList(ParameterListSyntax node)
    {
        var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        list.VisitUpTo(node.OpenParenToken, including: true);

        if (prependSelf)
        {
            sb.Append("self");
            prependSelf = false;

            if (node.Parameters.Count > 0)
            {
                sb.Append(", ");
            }
        }

        list.VisitUpTo(node.CloseParenToken); // we don't want whitepace after the close paren
        sb.Append(node.CloseParenToken.Text);
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        sb.Append(node.Identifier.Text);
    }

    // to ignore GIL attributes
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        sb.Append(node.ToFullString()); // otherwise we need to update how `VisitArgumentList()` works
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("match ");
        Visit(node.Expression);
        sb.AppendLine(":");

        foreach (var arm in node.Sections)
        {
            Visit(arm);
        }
    }

    // a section is like: `case EventId.DO: ROOT_do(); break;`
    public override void VisitSwitchSection(SwitchSectionSyntax node)
    {
        SwitchLabelSyntax label = node.Labels.Single(); // must be a single label for now

        Visit(label);

        foreach (var statement in node.Statements)
        {
            if (statement is BreakStatementSyntax)
            {
                // ignore break statements
            }
            else
            {
                Visit(statement);
            }
        }
    }

    public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
    {
        VisitToken(node.Keyword);
        sb.Append("self.");
        Visit(node.Value);
        VisitToken(node.ColonToken);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        bool done = false;

        done |= transpilerHelper.HandleGilSpecialInvocations(node, sb);
        done |= transpilerHelper.HandleGilUnusedVarSpecialInvocation(node, argument =>
        {
            // skip parent expression semicolon and end of line trivia
            ExpressionStatementSyntax expressionParent = (ExpressionStatementSyntax)node.Parent!;
            tokenToSkip = expressionParent.SemicolonToken;
        });

        if (!done)
        {
            base.VisitInvocationExpression(node);
        }
    }

    // kinda like: https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp/Syntax/InternalSyntax/SyntaxToken.cs,516c0eb61810c3ef,references
    public override void VisitToken(SyntaxToken token)
    {
        if (token == tokenToSkip)
        {
            tokenToSkip = null;
            return;
        }

        token.LeadingTrivia.VisitWith(this);

        string tokenText = token.Text;

        switch ((SyntaxKind)token.RawKind)
        {
            case SyntaxKind.ConstKeyword:
            case SyntaxKind.OpenBraceToken:
            case SyntaxKind.CloseBraceToken:
            case SyntaxKind.SemicolonToken:
            case SyntaxKind.ClassKeyword: tokenText = ""; break;
            case SyntaxKind.TrueKeyword: tokenText = "True"; break;
            case SyntaxKind.FalseKeyword: tokenText = "False"; break;
            case SyntaxKind.ThisKeyword: tokenText = "self"; break;
        }

        if (token.IsKind(SyntaxKind.ExclamationToken) && token.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression))
        {
            // ignore exclamations like: `this.current_state_exit_handler!();`
        }
        else
        {
            sb.Append(tokenText);
        }

        token.TrailingTrivia.VisitWith(this);
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        string str = trivia.ToString();

        if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
        {
            str = str.Replace("//", "#");
        }

        // this interferes with `/*<<<<<rm2<<<<<MainClass.trace...` special comments
        //else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
        //{
        //    str = str.Replace("/*", "\"\"\"");
        //    str = str.Replace("*/", "\"\"\"");
        //}

        sb.Append(str);

        // useful for nullable directives or maybe structured comments
        //if (trivia.HasStructure)
        //{
        //    this.Visit(trivia.GetStructure());
        //}
    }


}
