using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using Microsoft.CodeAnalysis.Formatting;
using Antlr4.Runtime;

#nullable enable

namespace StateSmith.Output.Gil.CSharp;

public class CSharpGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder sb;
    private readonly RenderConfigCSharpVars renderConfigCSharp;
    private readonly RenderConfigVars renderConfig;

    private SemanticModel Model => _model.ThrowIfNull();
    private SemanticModel? _model;

    private bool useStaticDelegates = true;    // could make this a user accessible setting
    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder? _methodPtrFinder;
    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder MethodPtrFinder => _methodPtrFinder.ThrowIfNull();

    public CSharpGilVisitor(StringBuilder sb, RenderConfigCSharpVars renderConfigCSharp, RenderConfigVars renderConfig) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigCSharp = renderConfigCSharp;
    }

    public void Process()
    {
        // get input gil code and then clear string buffer to hold new result
        string gilCode = sb.ToString();
        GilHelper.Compile(gilCode, out CompilationUnitSyntax root, out _model);
        sb.Clear();

        if (useStaticDelegates)
        {
            _methodPtrFinder = new(root, _model);
            MethodPtrFinder.Find();
        }

        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        if (renderConfigCSharp.UseNullable)
            sb.AppendLine($"#nullable enable");

        sb.AppendLineIfNotBlank(renderConfigCSharp.Usings);

        this.Visit(root);

        FormatOutput();
    }

    private void FormatOutput()
    {
        var outputCode = sb.ToString();
        sb.Clear();

        // note: we don't use the regular `NormalizeWhitespace()` as it tightens all code up, and actually messes up some indentation.
        outputCode = Formatter.Format(CSharpSyntaxTree.ParseText(outputCode).GetRoot(), new AdhocWorkspace()).ToFullString();
        sb.Append(outputCode);
    }

    // delegates are assumed to be method pointers
    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        if (!useStaticDelegates)
        {
            base.VisitDelegateDeclaration(node);
            return;
        }

        var symbol = Model.GetDeclaredSymbol(node).ThrowIfNull();

        WalkableChildSyntaxList walkableChildSyntaxList = new(this, node.ChildNodesAndTokens());
        walkableChildSyntaxList.VisitUpTo(node.ParameterList);

        sb.Append("(" + symbol.ContainingType.Name + " sm)");
        VisitToken(node.SemicolonToken);
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
        if (renderConfigCSharp.UseNullable)
        {
            base.VisitNullableType(node);
        }
        else
        {
            Visit(node.ElementType); // this avoids outputting the `?` for a nullable type
            sb.Append(' ');
        }
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (GilHelper.IsGilNoEmit(node))
            return;

        MaybeOutputStaticDelegate(node);

        base.VisitMethodDeclaration(node);
    }

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

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (GilHelper.HandleSpecialGilEmitClasses(node, this)) return;

        var iterableChildSyntaxList = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());

        iterableChildSyntaxList.VisitUpTo(SyntaxKind.ClassKeyword);

        if (renderConfigCSharp.UsePartialClass)
            sb.Append("partial ");

        iterableChildSyntaxList.VisitUpTo(node.Identifier);

        // handle identifier specially so that it doesn't put base list on newline
        iterableChildSyntaxList.Remove(node.Identifier);
        sb.Append(node.Identifier.Text);
        MaybeOutputBaseList();
        VisitTrailingTrivia(node.Identifier);

        iterableChildSyntaxList.VisitUpTo(node.OpenBraceToken, including: true);
        sb.AppendLineIfNotBlank(renderConfigCSharp.ClassCode);  // append class code after open brace token

        iterableChildSyntaxList.VisitRest();
    }

    private void MaybeOutputBaseList()
    {
        var baseList = renderConfigCSharp.BaseList.Trim();
        if (baseList.Length > 0)
            sb.Append(" : " + baseList);
    }

    // to ignore GIL attributes
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitArgumentList(ArgumentListSyntax node)
    {
        var invocation = (InvocationExpressionSyntax)node.Parent.ThrowIfNull();
        var iMethodSymbol = (IMethodSymbol)Model.GetSymbolInfo(invocation).ThrowIfNull().Symbol.ThrowIfNull();

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

        done |= GilHelper.HandleGilSpecialInvocations(node, sb);
        done |= GilHelper.HandleGilUnusedVarSpecialInvocation(node, argument =>
        {
            sb.Append(node.GetLeadingTrivia().ToFullString());
            sb.Append($"_ = {argument.ToFullString()}"); // trailing semi-colon is already part of parent ExpressionStatement
        });

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
        this.VisitLeadingTrivia(token);
        sb.Append(token.Text);
        this.VisitTrailingTrivia(token);
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
