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
    public readonly StringBuilder sb;
    private readonly RenderConfigTypeScriptVars renderConfigTypeScript;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;

    private bool skipLeadingTrivia = false;
    private bool publicAsExport = false;

    private SemanticModel model;


    // Balanced1 stuff that is not yet supported https://github.com/StateSmith/StateSmith/issues/407
    #region Not Yet Supported
    private bool useStaticDelegates = true;    // could make this a user accessible setting

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder? _methodPtrFinder;

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder MethodPtrFinder => _methodPtrFinder.ThrowIfNull();
    #endregion Not Yet Supported


    private SyntaxToken? tokenToSkip;

    public TypeScriptGilVisitor(string gilCode, StringBuilder sb, RenderConfigTypeScriptVars renderConfigTypeScript, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigTypeScript = renderConfigTypeScript;
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

        this.Visit(transpilerHelper.root);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        foreach (var kid in node.ChildNodes().OfType<EnumDeclarationSyntax>())
        {
            Visit(kid);
        }

        foreach (var kid in node.ChildNodes().OfType<ClassDeclarationSyntax>())
        {
            Visit(kid);
        }

        foreach (var kid in node.ChildNodes().OfType<FieldDeclarationSyntax>())
        {
            if (kid is FieldDeclarationSyntax field && field.IsConst())
                Visit(kid);
        }

        var iterableChildSyntaxList = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        publicAsExport = true;

        iterableChildSyntaxList.VisitUpTo(SyntaxKind.ClassKeyword);

        iterableChildSyntaxList.VisitUpTo(node.Identifier);

        // handle identifier specially so that it doesn't put base list on newline
        iterableChildSyntaxList.Remove(node.Identifier);
        sb.Append(node.Identifier.Text);
        MaybeOutputBaseList();
        VisitTrailingTrivia(node.Identifier);

        iterableChildSyntaxList.VisitUpTo(node.OpenBraceToken, including: true);
        sb.AppendLineIfNotBlank(renderConfigTypeScript.ClassCode);  // append class code after open brace token

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

    //public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
    //{
    //    // older versions of TypeScript don't like `case StateId.ROOT:` they want `case ROOT:`.
    //    // I got errors like: `error: constant expression required` and `error: case expressions must be constant expressions`
    //    VisitToken(node.Keyword);
    //    MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)node.Value; // we know it's a member access expression
    //    Visit(memberAccessExpressionSyntax.Name);
    //    VisitToken(node.ColonToken);
    //}

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        publicAsExport = true;
        base.VisitEnumDeclaration(node);
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

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilData(node))
            return;

        MaybeOutputStaticDelegate(node);

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


    //--------------------------------------------------------------------------------
    // Balanced1 stuff that is not yet supported https://github.com/StateSmith/StateSmith/issues/407
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
