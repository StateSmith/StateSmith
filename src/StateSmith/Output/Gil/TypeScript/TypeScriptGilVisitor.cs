#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System.Linq;
using System.Collections.Generic;

namespace StateSmith.Output.Gil.TypeScript;

/// <summary>
/// NOTE! This is mostly a copy of CSharp transpiler with some TypeScript specific changes.
/// It still needs some updating: https://github.com/StateSmith/StateSmith/issues/407
/// </summary>
public class TypeScriptGilVisitor : CSharpSyntaxWalker
{
    public StringBuilder sb;
    private readonly RenderConfigTypeScriptVars renderConfigTypeScript;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly CodeStyleSettings codeStyleSettings;

    private bool skipLeadingTrivia = false;
    private bool publicAsExport = false;
    private int nestedCount = 0;

    private SemanticModel model;


    // Balanced1 stuff that is not yet supported https://github.com/StateSmith/StateSmith/issues/407

    private SyntaxToken? tokenToSkip;

    public TypeScriptGilVisitor(string gilCode, StringBuilder sb, RenderConfigTypeScriptVars renderConfigTypeScript, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler, CodeStyleSettings codeStyleSettings) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigTypeScript = renderConfigTypeScript;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, roslynCompiler);
        model = transpilerHelper.model;
        this.codeStyleSettings = codeStyleSettings;
    }

    public void Process()
    {
        transpilerHelper.PreProcess();

        sb.AppendLineIfNotBlank(renderConfig.FileTop);

        this.Visit(transpilerHelper.root);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        bool thisIsFsmClass = nestedCount == 0;

        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;
        HoistTopLevelMembersOutOfClass(node);

        var iterableChildSyntaxList = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        publicAsExport = true;

        if (thisIsFsmClass)
        {
            sb.AppendLine(); // needs a bit of space from hoisted members
        }

        iterableChildSyntaxList.VisitUpTo(SyntaxKind.ClassKeyword);

        iterableChildSyntaxList.VisitUpTo(node.Identifier);

        // handle identifier specially so that it doesn't put base list on newline
        iterableChildSyntaxList.Remove(node.Identifier);
        sb.Append(node.Identifier.Text);

        if (thisIsFsmClass)
        {
            MaybeOutputBaseList();
        }
        VisitTrailingTrivia(node.Identifier);

        iterableChildSyntaxList.VisitUpTo(node.OpenBraceToken, including: true);

        if (thisIsFsmClass && !string.IsNullOrWhiteSpace(renderConfigTypeScript.ClassCode))
        {
            var code = StringUtils.Indent(renderConfigTypeScript.ClassCode, codeStyleSettings.Indent1);
            sb.AppendLine(code);
        }

        foreach (var kid in node.ChildNodes().OfType<FieldDeclarationSyntax>())
        {
            if (kid is FieldDeclarationSyntax field && !field.IsConst())
                Visit(kid);
        }

        var methods = node.ChildNodes().Where(n => n is MethodDeclarationSyntax || n is ConstructorDeclarationSyntax);
        foreach (var method in methods)
        {
            Visit(method);
        }

        VisitToken(node.CloseBraceToken);
    }

    /// <summary>
    /// TypeScript doesn't support nested classes, or enums declared in classes.
    /// </summary>
    /// <param name="node"></param>
    private void HoistTopLevelMembersOutOfClass(ClassDeclarationSyntax node)
    {
        nestedCount++;
        {
            var old = sb;
            sb = new StringBuilder();

            foreach (var kid in node.ChildNodes().Where(n => n is EnumDeclarationSyntax || n is ClassDeclarationSyntax || n is FieldDeclarationSyntax))
            {
                if (kid is FieldDeclarationSyntax field)
                {
                    if (field.IsConst())
                    {
                        Visit(kid);
                    }
                }
                else
                {
                    Visit(kid);
                }
            }

            string code = sb.ToString();
            code = StringUtils.DeIndent(code);
            sb = old;
            sb.Append(code);
        }

        nestedCount--;
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialFieldDeclarations(node, sb))
            return;

        publicAsExport = node.IsConst();

        base.VisitFieldDeclaration(node);
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("let ");
        base.VisitLocalDeclarationStatement(node);
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        VariableDeclaratorSyntax variableDeclaratorSyntax = node.Variables.Single();
        sb.Append($"{variableDeclaratorSyntax.Identifier}:");
        sb.Append(node.Type.GetTrailingTrivia());
        skipLeadingTrivia = true;
        Visit(node.Type);
        StringUtils.EraseTrailingWhitespace(sb);

        if (variableDeclaratorSyntax.Initializer != null)
        {
            VisitTrailingTrivia(variableDeclaratorSyntax.Identifier);
            Visit(variableDeclaratorSyntax.Initializer);
        }
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        publicAsExport = true;
        base.VisitEnumDeclaration(node);
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

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilData(node))
            return;

        publicAsExport = false;

        var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        list.VisitUpTo(node.ReturnType);
        list.SkipNext(); // skip return type
        list.VisitUpTo(node.Body.ThrowIfNull());
        StringUtils.EraseTrailingWhitespace(sb);
        sb.Append(": ");
        Visit(node.ReturnType);
        sb.AppendLine();
        list.VisitRest();
    }

    private void MaybeOutputBaseList()
    {
        var extends = renderConfigTypeScript.Extends.Trim();
        if (extends.Length > 0)
        {
            sb.Append(" extends " + extends);
        }

        var implements = renderConfigTypeScript.Implements.Trim();
        if (implements.Length > 0)
        {
            sb.Append(" implements " + implements);
        }
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

    public override void VisitParameter(ParameterSyntax node)
    {
        sb.Append(node.Identifier);
        sb.Append(": ");
        Visit(node.Type);
        StringUtils.EraseTrailingWhitespace(sb);
    }

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialExpressionStatements(node, sb))
            return;

        base.VisitExpressionStatement(node);
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

        if (skipLeadingTrivia)
        {
            skipLeadingTrivia = false;
        }
        else
        {
            token.LeadingTrivia.VisitWith(this);
        }

        string tokenText = token.Text;

        switch ((SyntaxKind)token.RawKind)
        {
            case SyntaxKind.BoolKeyword: tokenText = "boolean"; break;
            case SyntaxKind.IntKeyword: tokenText = "number"; break;
            case SyntaxKind.PublicKeyword:
                if (publicAsExport)
                {
                    tokenText = "export";
                    //publicAsExport = false;
                }
                break;
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
        sb.Append(trivia.ToString());

        // useful for nullable directives or maybe structured comments
        //if (trivia.HasStructure)
        //{
        //    this.Visit(trivia.GetStructure());
        //}
    }
}
