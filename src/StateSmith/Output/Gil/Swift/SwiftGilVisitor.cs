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
using System.ComponentModel;
using System.Reflection.Metadata;

namespace StateSmith.Output.Gil.Swift;

/// <summary>
/// NOTE! This is mostly a copy of CSharp transpiler with some Swift specific changes.
/// </summary>
public class SwiftGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder sb;
    private readonly RenderConfigSwiftVars renderConfigSwift;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly CodeStyleSettings codeStyleSettings;
    private string Indent => codeStyleSettings.Indent1;

    private SemanticModel model;

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

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        var iterableChildSyntaxList = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());

        iterableChildSyntaxList.VisitUpTo(SyntaxKind.ClassKeyword);

        iterableChildSyntaxList.VisitUpTo(node.Identifier);

        // handle identifier specially so that it doesn't put base list on newline
        iterableChildSyntaxList.Remove(node.Identifier);
        sb.Append(node.Identifier.Text);
        MaybeOutputBaseList();
        VisitTrailingTrivia(node.Identifier);

        iterableChildSyntaxList.VisitUpTo(node.OpenBraceToken, including: true);
        sb.AppendLineIfNotBlank(renderConfigSwift.ClassCode);  // append class code after open brace token

        iterableChildSyntaxList.VisitRest();
    }

    private void MaybeOutputBaseList()
    {
        var baseList = renderConfigSwift.BaseList.Trim();
        if (baseList.Length > 0)
            sb.Append(" : " + baseList);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);

        
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }

        if (node.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            sb.Append("public ");
        }

        sb.Append("enum ");        
        sb.Append(node.Identifier.Text);
        sb.Append(" : "); 
        if (node.BaseList != null)
        {
            Visit(node.BaseList);
        } 
        else
        {
            sb.Append("Int");
        }
        sb.AppendLine();

        VisitToken(node.OpenBraceToken);

        foreach (var member in node.Members)
        {
            Visit(member);
            sb.AppendLine();
        }

        VisitToken(node.CloseBraceToken);
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("case ");
        sb.Append(node.Identifier.Text);
        if (node.EqualsValue != null)
        {
            sb.Append(" ");
            Visit(node.EqualsValue);
        }
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialFieldDeclarations(node, sb))
            return;

        base.VisitFieldDeclaration(node);
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        var variable = node.Variables.Single();
        if (node.Parent is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ConstKeyword))
            sb.Append("let ");
        else
            sb.Append("var ");
        VisitToken(variable.Identifier);
        if (node.Type != null)
        {
            sb.Append(": ");
            Visit(node.Type);
        }
        if (variable.Initializer != null)
        {          
            Visit(variable.Initializer);
        }
        else
        {
            var fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (fieldSymbol != null && !fieldSymbol.Type.IsReferenceType)
            {
                var defaultValue = GetDefaultValue(fieldSymbol.Type);
                sb.Append(" = ");
                sb.Append(defaultValue);
            }
            
        }
        VisitTrailingTrivia(node.GetLastToken());
    }

    private string GetDefaultValue(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            return ".init(rawValue: 0)!";
        }
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean => "false",
            SpecialType.System_Char => "'\\0'",
            _ => "0"
        };
    }

    // handle object creation like `new MyClass()`
    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        Visit(node.Type);
        //Visit(node.ArgumentList);  // we need to update how `VisitArgumentList()` works
        sb.Append("()");
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }

        if (node.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            sb.Append("public ");
        }

        sb.Append("init");

        Visit(node.ParameterList);
        
        Visit(node.Body);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilData(node))
            return;

        bool isStatic = node.Modifiers.Any(SyntaxKind.StaticKeyword);

        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);

        //if private, add a leading underscore
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }

        if (node.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            sb.Append("public ");
        }
        
        if (isStatic)
        {
            sb.Append("static ");
        }

        sb.Append("func ");

        VisitToken(node.Identifier);

        VisitParameterList(node.ParameterList);

        if (node.ReturnType != null && (node.ReturnType as PredefinedTypeSyntax)?.Keyword.RawKind != (int)SyntaxKind.VoidKeyword)
        {
            sb.Append(indent);
            sb.Append(" -> ");
            Visit(node.ReturnType);
        }

        VisitBlock(node.Body!);
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        sb.Append("_ ");
        VisitToken(node.Identifier);
        if (node.Type != null)
        {
            sb.Append(" : ");
            Visit(node.Type!);
        }
        if (node.Default != null)
        {
            Visit(node.Default);
        }
    }

    // to ignore GIL attributes
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitBlock(BlockSyntax node)
    {
        void VisitStatements(IEnumerable<StatementSyntax> statements)
        {
            foreach (var statement in statements)
            {
                if (statement is BlockSyntax block)
                {
                    VisitLeadingTrivia(block.GetFirstToken());
                    string indent = StringUtils.FindLastIndent(sb);
                    sb.AppendLine("do {");
                    VisitStatements(block.Statements);
                    sb.Append(indent);
                    sb.Append("} catch {}");
                    VisitTrailingTrivia(block.GetLastToken());
                }
                else
                    Visit(statement);
            }
        }

        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);
        sb.AppendLine("{");
        VisitStatements(node.Statements);
        sb.Append(indent);
        sb.Append('}');
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialExpressionStatements(node, sb))
            return;

        base.VisitExpressionStatement(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("if ");
        Visit(node.Condition);
        sb.AppendLine();
        Visit(node.Statement);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {        
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("while ");
        Visit(node.Condition);
        sb.AppendLine();
        Visit(node.Statement);
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    { 
        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);
        sb.Append("switch ");
        Visit(node.Expression);
        sb.AppendLine();
        sb.Append(indent);
        sb.AppendLine("{");
        foreach (var section in node.Sections) {
            Visit(section);
        }

        if (!node.Sections.SelectMany(s => s.Labels).Any(l => l.Keyword.RawKind == (int)SyntaxKind.DefaultKeyword))
        {
            sb.Append(indent);
            sb.Append(Indent);
            sb.AppendLine("default: break");
        }

        sb.Append(indent);
        sb.Append("}");
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitSwitchSection(SwitchSectionSyntax node)
    {
        Visit(node.Labels[0]);
        foreach (var label in node.Labels.Skip(1))
        {
            sb.Append(", ");
            Visit(label);
        }
        var breakIndex = node.Statements.TakeWhile((s) => s is not BreakStatementSyntax).Count();
        var returnIndex = node.Statements.TakeWhile((s) => s is not ReturnStatementSyntax).Count();
        if (breakIndex == node.Statements.Count && returnIndex == node.Statements.Count)
        {
            foreach (var statement in node.Statements)
            {
                Visit(statement);
            }
            sb.Append(";fallthrough");
            sb.AppendLine();
        } 
        else if (breakIndex < returnIndex)
        {
            foreach (var statement in node.Statements.Take(Math.Max(1, breakIndex)))
            {
                Visit(statement);
            }
            VisitTrailingTrivia(node.Statements.Last().GetLastToken());
        }
        else
        {
            foreach (var statement in node.Statements.Take(returnIndex + 1))
            {
                Visit(statement);
            }
        }
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        bool done = false;

        done |= transpilerHelper.HandleGilSpecialInvocations(node, sb);
        done |= transpilerHelper.HandleGilUnusedVarSpecialInvocation(node, argument => {});

        if (!done)
        {
            base.VisitInvocationExpression(node);
        }
    }

    // kinda like: https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp/Syntax/InternalSyntax/SyntaxToken.cs,516c0eb61810c3ef,references
    public override void VisitToken(SyntaxToken token)
    {
        token.LeadingTrivia.VisitWith(this);

        string tokenText = token.Text;

        switch ((SyntaxKind)token.RawKind)
        {
            case SyntaxKind.ConstKeyword:
            case SyntaxKind.SemicolonToken: tokenText = ""; break;
            case SyntaxKind.NullKeyword: tokenText = "nil"; break;
            case SyntaxKind.TrueKeyword: tokenText = "true"; break;
            case SyntaxKind.FalseKeyword: tokenText = "false"; break;
            case SyntaxKind.ThisKeyword: tokenText = "self"; break;
            case SyntaxKind.IntKeyword: tokenText = "Int"; break;
            case SyntaxKind.StringKeyword: tokenText = "String"; break;
            case SyntaxKind.BoolKeyword: tokenText = "Bool"; break;
            case SyntaxKind.FloatKeyword: tokenText = "Float"; break;
            case SyntaxKind.DoubleKeyword: tokenText = "Double"; break;
            case SyntaxKind.VoidKeyword: tokenText = "Void"; break;
        }

        sb.Append(tokenText);

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
