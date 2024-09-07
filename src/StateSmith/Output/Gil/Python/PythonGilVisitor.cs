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

    private SemanticModel model;
    private string Indent => codeStyleSettings.Indent1;


    // Balanced1 stuff that is not yet supported https://github.com/StateSmith/StateSmith/issues/398
    #region Not Yet Supported
    private bool useStaticDelegates = true;    // could make this a user accessible setting

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder? _methodPtrFinder;

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder MethodPtrFinder => _methodPtrFinder.ThrowIfNull();
    #endregion Not Yet Supported


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
        if (useStaticDelegates)
        {
            _methodPtrFinder = new(transpilerHelper.root, transpilerHelper.model);
            MethodPtrFinder.Find();
        }

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

    public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
    {
        VisitToken(node.Keyword);
        MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)node.Value; // we know it's a member access expression
        Visit(memberAccessExpressionSyntax.Name);
        VisitToken(node.ColonToken);
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (useStaticDelegates && MethodPtrFinder.identifiers.Contains(node))
        {
            sb.Append("ptr_" + node.Identifier.Text);
        }
        else
        {
            base.VisitIdentifierName(node);
        }
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

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        bool done = false;

        if (transpilerHelper.HandleThisMethodAccess(node))
            done = true;

        if (!done)
            base.VisitMemberAccessExpression(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilNoEmit(node))
            return;

        MaybeOutputStaticDelegate(node);

        base.VisitMethodDeclaration(node);
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

    public override void VisitArgumentList(ArgumentListSyntax node)
    {
        var invocation = (InvocationExpressionSyntax)node.Parent.ThrowIfNull();
        var iMethodSymbol = (IMethodSymbol)model.GetSymbolInfo(invocation).ThrowIfNull().Symbol.ThrowIfNull();

        if (useStaticDelegates && !iMethodSymbol.IsStatic && iMethodSymbol.MethodKind == MethodKind.DelegateInvoke)
        {
            var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
            list.VisitUpTo(node.OpenParenToken, including: true);

            sb.Append("this");
            if (node.Arguments.Count > 0)
            {
                sb.Append(", ");
            }

            list.VisitRest();
        }
        else
        {
            base.VisitArgumentList(node);
        }
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

        // TODO - support in Python.
        //if (!done)
        //{
        //    var symbol = Model.GetSymbolInfo(node).Symbol;
        //    if (symbol is INamedTypeSymbol namedTypeSymbol)
        //    {
        //        if (namedTypeSymbol.DelegateInvokeMethod != null)
        //        {
        //            done = true;
        //        }
        //    }
        //}

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
            case SyntaxKind.ClassKeyword: tokenText = ""; break;
            case SyntaxKind.StructKeyword: tokenText = "class"; break;
            case SyntaxKind.ConstKeyword: tokenText = ""; break;
            case SyntaxKind.StringKeyword: tokenText = "String"; break;
            case SyntaxKind.BoolKeyword: tokenText = "boolean"; break;
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
        else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
        {
            str = str.Replace("/*", "\"\"\"");
            str = str.Replace("*/", "\"\"\"");
        }

        sb.Append(str);

        // useful for nullable directives or maybe structured comments
        //if (trivia.HasStructure)
        //{
        //    this.Visit(trivia.GetStructure());
        //}
    }


    //--------------------------------------------------------------------------------
    // Balanced1 stuff that is not yet supported https://github.com/StateSmith/StateSmith/issues/395
    //--------------------------------------------------------------------------------
    #region Not Yet Supported

    /// <summary>
    /// Why do this? See https://github.com/StateSmith/StateSmith/wiki/GIL----Generic-Intermediate-Language
    /// </summary>
    private void MaybeOutputStaticDelegate(MethodDeclarationSyntax node)
    {
        if (!useStaticDelegates || !MethodPtrFinder.methods.Contains(node))
            return;

        var symbol = MethodPtrFinder.delegateSymbol.ThrowIfNull();
        sb.AppendLine();
        sb.AppendLine("// static delegate to avoid implicit conversion and garbage collection");
        sb.Append($"private static readonly {symbol.Name} ptr_{node.Identifier} = ({symbol.ContainingType.Name} sm) => sm.{node.Identifier}();");
    }

    // delegates are assumed to be method pointers
    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        if (!useStaticDelegates)
        {
            base.VisitDelegateDeclaration(node);
            return;
        }

        var symbol = model.GetDeclaredSymbol(node).ThrowIfNull();

        WalkableChildSyntaxList walkableChildSyntaxList = new(this, node.ChildNodesAndTokens());
        walkableChildSyntaxList.VisitUpTo(node.ParameterList);

        sb.Append("(" + symbol.ContainingType.Name + " sm)");
        VisitToken(node.SemicolonToken);
    }

    #endregion Not Yet Supported
}
