#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;

namespace StateSmith.Output.Gil.Java;

/// <summary>
/// NOTE! This is mostly a copy of CSharp transpiler with some Java specific changes.
/// It still needs some updating: https://github.com/StateSmith/StateSmith/issues/395
/// </summary>
public class JavaGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder sb;
    private readonly RenderConfigJavaVars renderConfigJava;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;

    private SemanticModel model;


    // Balanced1 stuff that is not yet supported https://github.com/StateSmith/StateSmith/issues/395
    #region Not Yet Supported
    private bool useStaticDelegates = true;    // could make this a user accessible setting

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder? _methodPtrFinder;

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder MethodPtrFinder => _methodPtrFinder.ThrowIfNull();
    #endregion Not Yet Supported


    private SyntaxToken? tokenToSkip;

    public JavaGilVisitor(string gilCode, StringBuilder sb, RenderConfigJavaVars renderConfigJava, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigJava = renderConfigJava;
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

        var package = renderConfigJava.Package.Trim();
        if (package.Length > 0)
        {
            sb.AppendLine("package " + renderConfigJava.Package + ";");
            sb.AppendLine();
        }

        sb.AppendLineIfNotBlank(renderConfigJava.Imports, optionalTrailer: "\n");

        this.Visit(transpilerHelper.root);
    }

    public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
    {
        // older versions of Java don't like `case StateId.ROOT:` they want `case ROOT:`.
        // I got errors like: `error: constant expression required` and `error: case expressions must be constant expressions`
        VisitToken(node.Keyword);
        MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)node.Value; // we know it's a member access expression
        Visit(memberAccessExpressionSyntax.Name);
        VisitToken(node.ColonToken);
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        // we don't want to output identifier trailing space so we do this manually
        // we want to output `ROOT,` instead of `ROOT ,`. The comma is handled by the parent node.
        VisitLeadingTrivia(node.Identifier);
        sb.Append(node.Identifier.Text);
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
        sb.AppendLineIfNotBlank(renderConfigJava.ClassCode);  // append class code after open brace token

        iterableChildSyntaxList.VisitRest();
    }

    private void MaybeOutputBaseList()
    {
        var extends = renderConfigJava.Extends.Trim();
        if (extends.Length > 0)
        {
            sb.Append(" extends " + extends);
        }

        var implements = renderConfigJava.Implements.Trim();
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

        // TODO - support in Java.
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
            case SyntaxKind.StructKeyword: tokenText = "class"; break;
            case SyntaxKind.ConstKeyword: tokenText = "final"; break;
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
        sb.Append(trivia.ToString());

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
