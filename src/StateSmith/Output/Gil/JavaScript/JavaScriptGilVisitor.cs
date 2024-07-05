using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System.Linq;

#nullable enable

namespace StateSmith.Output.Gil.JavaScript;

public class JavaScriptGilVisitor : CSharpSyntaxWalker
{
    private readonly StringBuilder sb;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly RenderConfigJavaScriptVars renderConfigJavaScript;

    private readonly SemanticModel model;
    private readonly GilTranspilerHelper transpilerHelper;
    private SyntaxToken? tokenToSkip;

    public JavaScriptGilVisitor(string gilCode, StringBuilder fileSb, RenderConfigBaseVars renderConfig, RenderConfigJavaScriptVars renderConfigJavaScript, RoslynCompiler roslynCompiler) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = fileSb;
        this.renderConfig = renderConfig;
        this.renderConfigJavaScript = renderConfigJavaScript;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, roslynCompiler);
        model = transpilerHelper.model;
    }

    public void Process()
    {
        transpilerHelper.PreProcess();

        sb.AppendLineIfNotBlank(renderConfig.FileTop, optionalTrailer:"\n");
        this.Visit(transpilerHelper.root);
    }

    // delegates are assumed to be method pointers
    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        // nothing required for js
    }

    public override void VisitNullableType(NullableTypeSyntax node)
    {
        Visit(node.ElementType); // this avoids outputting the `?` for a nullable type
        sb.Append(' ');
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilNoEmit(node))
            return;

        VisitLeadingTrivia(node.GetFirstToken());

        var kids = new WalkableChildSyntaxList(this, node);
        kids.SkipUpTo(node.Identifier, including: true);

        IMethodSymbol symbol = model.GetDeclaredSymbol(node).ThrowIfNull();

        if (symbol.IsStatic)
            sb.Append("static ");

        sb.Append(GetJsName(symbol));
        kids.VisitRest();
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        var kidsList = new WalkableChildSyntaxList(this, node);

        kidsList.VisitUpTo(SyntaxKind.ClassKeyword);

        if (renderConfigJavaScript.UseExportOnClass)
        {
            sb.Append("export ");
        }

        kidsList.VisitUpTo(node.Identifier, including: false);
        kidsList.SkipNext();

        VisitLeadingTrivia(node.Identifier);
        sb.Append(node.Identifier.Text);
        MaybeOutputExtends();
        VisitTrailingTrivia(node.Identifier);

        kidsList.VisitUpTo(node.OpenBraceToken, including: true);
        sb.AppendLineIfNotBlank(renderConfigJavaScript.ClassCode);  // append class code after open brace token

        // output enums and fields
        foreach (var kid in node.ChildNodes())
        {
            if (kid is EnumDeclarationSyntax || kid is FieldDeclarationSyntax)
                Visit(kid);
        }

        // output methods
        foreach (var kid in node.ChildNodes().OfType<MethodDeclarationSyntax>())
        {
            Visit(kid);
        }

        node.ChildNodes().OfType<ConstructorDeclarationSyntax>().Single().VisitWith(this);

        VisitToken(node.CloseBraceToken);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        bool outputCtor = false;
        if (outputCtor)
        {
            VisitLeadingTrivia(node.GetFirstToken());

            sb.Append($"constructor()");
            node.ParameterList.CloseParenToken.TrailingTrivia.VisitWith(this);
            node.Body.ThrowIfNull().OpenBraceToken.VisitWith(this);

            node.Body.CloseBraceToken.VisitWith(this);
        }
    }

    private void MaybeOutputExtends()
    {
        var extends = renderConfigJavaScript.ExtendsSuperClass.Trim();
        if (extends.Length > 0)
            sb.Append(" extends " + extends);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        var kidsList = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        kidsList.SkipUpTo(node.EnumKeyword, including: true);

        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append($"static ");
        kidsList.VisitUpTo(node.Identifier);
        kidsList.Remove(node.Identifier);
        sb.Append(node.Identifier.Text);
        sb.Append(" = ");
        VisitTrailingTrivia(node.Identifier);
        kidsList.VisitRest();

        sb.AppendLine($"    static {{ Object.freeze(this.{node.Identifier.Text}); }}");
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        VisitToken(node.Identifier);
        sb.Append(": " + node.EqualsValue.ThrowIfNull().Value);
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());

        if (node.IsConst())
        {
            VariableDeclaratorSyntax varDecl = node.Declaration.Variables.Single();

            sb.Append("static ");
            varDecl.VisitWith(this);
            node.GetLastToken().VisitWith(this);

            sb.AppendLine($"    static {{ Object.freeze(this.{varDecl.Identifier.Text}); }}");
        }
        else
        {
            node.Declaration.VisitWith(this);
            node.GetLastToken().VisitWith(this);
        }
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var symbol = (INamedTypeSymbol)model.GetSymbolInfo(node.Type).Symbol.ThrowIfNull();

        if (symbol.TypeKind == TypeKind.Struct)
        {
            sb.Append("{\n");

            var fields = symbol.GetMembers().OfType<IFieldSymbol>();

            foreach (var field in fields)
            {
                sb.AppendLine("        " + field.Name + ": undefined,");
            }

            var structDeclarationSyntax = (StructDeclarationSyntax)(symbol.DeclaringSyntaxReferences.Single().GetSyntax());

            // this outputs any user vars that are written in comments awaiting post processing
            structDeclarationSyntax.CloseBraceToken.LeadingTrivia.VisitWith(this);
            sb.Append('}');
        }
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        node.GetLeadingTrivia().VisitWith(this);

        if (node.Parent is LocalDeclarationStatementSyntax)
        {
            sb.Append("let ");
        }

        node.Variables.Single().VisitWith(this);
    }

    public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        node.GetLeadingTrivia().VisitWith(this);

        if (node.FirstAncestorOrSelf<FieldDeclarationSyntax>().IsConst())
        {
            sb.Append(node.Identifier);
            VisitTrailingTrivia(node.Identifier);
        }
        else
        {
            sb.Append(GetJsName(model.GetDeclaredSymbol(node).ThrowIfNull()));
            node.Identifier.TrailingTrivia.VisitWith(this);
        }

        Visit(node.Initializer);
    }

    public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
    {
        base.VisitEqualsValueClause(node);
    }

    public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
    {
        var rank = node.DescendantNodes().OfType<ArrayRankSpecifierSyntax>().Single();
        sb.Append($"Array(");
        rank.Sizes.Single().VisitWith(this);    // no 2d arrays for now
        sb.Append($").fill(undefined)");
    }

    // to ignore GIL attributes
    // todo_low - check if can be removed.
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
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

    public override void VisitArgumentList(ArgumentListSyntax node)
    {
        bool isDelegateInvoke = false;

        if (((IMethodSymbol?)(model.GetSymbolInfo(node.Parent.ThrowIfNull()).Symbol))?.MethodKind == MethodKind.DelegateInvoke)
        {
            // js doesn't bind an instance to a delegate method.
            // we have to use `delegateMethod.call(this)`.
            isDelegateInvoke = true;
            sb.Append(".call");
        }

        var kids = new WalkableChildSyntaxList(this, node);
        kids.VisitUpTo(node.OpenParenToken, including: true);

        if (isDelegateInvoke)
        {
            sb.Append("this");
            if (node.Arguments.Count > 0)
                sb.Append(", ");
        }

        kids.VisitRest();
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        //sb.Append("/** @type {");
        //node.Type.ThrowIfNull().VisitWith(this);
        //sb.Append(PostProcessor.trimHorizontalWhiteSpaceMarker);
        //sb.Append("} */ ");
        node.Identifier.VisitWith(this);
    }

    public override void VisitCastExpression(CastExpressionSyntax node)
    {
        if (transpilerHelper.IsEnumMemberConversionToInt(node))
        {
            // just visit expression so we omit int cast
            // `(int32_t)event_id` ---> `event_id`
            Visit(node.Expression);
        }
        else
        {
            base.VisitCastExpression(node);
        }
    }

    //--------------------------------------------

    public override void VisitToken(SyntaxToken token)
    {
        if (token == tokenToSkip)
        {
            tokenToSkip = null;
            return;
        }

        token.LeadingTrivia.VisitWith(this);

        switch ((SyntaxKind)token.RawKind)
        {
            case SyntaxKind.PublicKeyword:
            case SyntaxKind.EnumKeyword:
            case SyntaxKind.ReadOnlyKeyword:
            case SyntaxKind.PrivateKeyword:
            case SyntaxKind.ConstKeyword:
            case SyntaxKind.VoidKeyword:
            case SyntaxKind.IntKeyword:
                return;
        }

        if (token.IsKind(SyntaxKind.ExclamationToken) && token.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression))
        {
            // ignore exclamations like: `this.current_state_exit_handler!();`
        }
        //else if (token.IsKind(SyntaxKind.IdentifierToken))
        //{
            
        //}
        else
        {
            sb.Append(token);
        }

        token.TrailingTrivia.VisitWith(this);
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var result = node.Identifier.Text;

        switch (result)
        {
            //case "Boolean": result = "bool"; break;

            default:
                {
                    SymbolInfo symbol = model.GetSymbolInfo(node);
                    result = GetJsName(symbol.Symbol.ThrowIfNull());
                    break;
                }
        }

        node.VisitLeadingTriviaWith(this);
        sb.Append(result);
        node.VisitTrailingTriviaWith(this);
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

    private string GetJsName(ISymbol symbol)
    {
        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (!fieldSymbol.IsStatic && !fieldSymbol.IsConst)
            {
                var result = "";
                if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
                    result = renderConfigJavaScript.PrivatePrefix;

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
                result = renderConfigJavaScript.PrivatePrefix;

            result += methodSymbol.Name;
            return result;
        }

        var fqn = transpilerHelper.GetFQN(symbol);
        return fqn;
    }
}
