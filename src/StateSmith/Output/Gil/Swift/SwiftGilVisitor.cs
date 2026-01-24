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

namespace StateSmith.Output.Gil.Swift;

/// <summary>
/// NOTE! This is mostly a copy of CSharp transpiler with some Swift specific changes.
/// </summary>
public class SwiftGilVisitor : CSharpSyntaxWalker
{
    private StringBuilder sb;
    private readonly RenderConfigSwiftVars renderConfigSwift;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly CodeStyleSettings codeStyleSettings;

    private SemanticModel model;
    private string Indent => codeStyleSettings.Indent1;

    private SyntaxToken? tokenToSkip;

    public SwiftGilVisitor(string gilCode, StringBuilder sb, RenderConfigSwiftVars renderConfigSwift, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler, CodeStyleSettings codeStyleSettings) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigSwift = renderConfigSwift;
        this.codeStyleSettings = codeStyleSettings;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, roslynCompiler);
        model = transpilerHelper.model;
    }

    public void Process()
    {
        transpilerHelper.PreProcess();

        sb.AppendLineIfNotBlank(renderConfig.FileTop);

        sb.AppendLine("import Foundation");

        sb.AppendLineIfNotBlank(renderConfigSwift.Imports, optionalTrailer: "\n");

        this.Visit(transpilerHelper.root);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("enum ");
        sb.Append(node.Identifier.Text);
        sb.Append(" {");
        sb.AppendLine();

        foreach (var member in node.Members)
        {
            sb.Append("case ");
            Visit(member);
            sb.AppendLine();
        }

        sb.Append("}");
        sb.AppendLine();
    }

    // handle object creation like `new MyClass()`
    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("self.");
        Visit(node.Type);
        //Visit(node.ArgumentList);  // we need to update how `VisitArgumentList()` works
        sb.Append("()");
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilData(node))
            return;

        bool isStatic = node.Modifiers.Any(SyntaxKind.StaticKeyword);

        VisitLeadingTrivia(node.GetFirstToken());

        //if private, add a leading underscore
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }
        
        if (isStatic)
        {
            sb.Append("static ");
        }

        sb.Append("func ");

        sb.Append(node.Identifier.Text);

        VisitParameterList(node.ParameterList);
        sb.Append('{');

        VisitBlock(node.Body!);
        
        sb.Append('}');
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

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialExpressionStatements(node, sb))
            return;

        base.VisitExpressionStatement(node);
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
}
