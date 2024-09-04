#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System;
using System.Text;
using StateSmith.Common;
using System.Linq;
using StateSmith.Output.UserConfig;

// spell-checker: ignore customizer

namespace StateSmith.Output.Gil.C99;

// TODO use a single string for `sm` as replacement for `this`. currently scattered.

public class C99GenVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder hFileSb;
    public readonly StringBuilder cFileSb;

    public StringBuilder privateSb = new();
    public StringBuilder publicSb = new();
    public StringBuilder sb;
    protected readonly SemanticModel model;
    protected bool renderingPrototypes = false;
    protected readonly RenderConfigBaseVars renderConfig;
    protected readonly RenderConfigCVars renderConfigC;
    protected readonly IGilToC99Customizer customizer;
    protected readonly GilTranspilerHelper transpilerHelper;
    protected readonly IncludeGuardProvider includeGuardProvider;

    public C99GenVisitor(SemanticModel model, StringBuilder hFileSb, StringBuilder cFileSb, RenderConfigBaseVars renderConfig, RenderConfigCVars renderConfigC, IGilToC99Customizer customizer, IncludeGuardProvider includeGuardProvider) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.model = model;
        this.hFileSb = hFileSb;
        this.cFileSb = cFileSb;
        this.renderConfig = renderConfig;
        this.renderConfigC = renderConfigC;
        this.customizer = customizer;
        this.includeGuardProvider = includeGuardProvider;

        transpilerHelper = new(this, model);

        sb = hFileSb;
    }

    public override void VisitCompilationUnit(CompilationUnitSyntax node)
    {
        OutputHFileTopSections();
        OutputCFileTopSections();

        sb = cFileSb;
        this.DefaultVisit(node);

        OutputFileBottomSections();
    }

    private void OutputHFileTopSections()
    {
        sb = hFileSb;
        transpilerHelper.PreProcess();
        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        sb.AppendLineIfNotBlank(renderConfigC.HFileTop);

        includeGuardProvider.OutputIncludeGuardTop(sb);
        sb.AppendLineIfNotBlank(renderConfigC.HFileTopPostIncludeGuard);

        sb.AppendLine("#include <stdint.h>");
        sb.AppendLineIfNotBlank(renderConfigC.HFileIncludes);
        sb.AppendLine();
    }

    private void OutputCFileTopSections()
    {
        sb = cFileSb;
        transpilerHelper.PreProcess();
        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        sb.AppendLineIfNotBlank(renderConfigC.CFileTop);
        sb.AppendLine($"#include \"{customizer.MakeHFileName()}\"");
        if (renderConfigC.UseStdBool)
        {
            sb.AppendLine("#include <stdbool.h> // required for `consume_event` flag");
        }
        sb.AppendLine("#include <string.h> // for memset");
        sb.AppendLineIfNotBlank(renderConfigC.CFileIncludes);
        sb.AppendLine();
    }

    private void OutputFileBottomSections()
    {
        hFileSb.AppendLineIfNotBlank(renderConfigC.HFileBottomPreIncludeGuard);
        includeGuardProvider.OutputIncludeGuardBottom(hFileSb);
        hFileSb.AppendLineIfNotBlank(renderConfigC.HFileBottom);

        cFileSb.AppendLineIfNotBlank(renderConfigC.CFileBottom);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        bool done = false;

        done |= transpilerHelper.HandleGilSpecialInvocations(node, sb);
        done |= transpilerHelper.HandleGilUnusedVarSpecialInvocation(node, argument =>
        {
            sb.Append(node.GetLeadingTrivia().ToFullString());
            sb.Append($"(void)");   // trailing semi-colon is already part of parent ExpressionStatement
            Visit(argument); // may be "this" or "event_id". Stuff like that.
        });

        if (!done)
        {
            base.VisitInvocationExpression(node);
        }
    }

    // to ignore GIL attributes
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        string name = GetCName(node);
        sb = hFileSb;
        sb.AppendLine($"");
        OutputStruct(node, name, outputTypedef: true);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        string name = GetCName(node);
        sb = hFileSb;

        // output enums and constants
        foreach (var kid in node.ChildNodes())
        {
            if (kid is EnumDeclarationSyntax || kid is FieldDeclarationSyntax field && field.IsConst())
                Visit(kid);
        }

        sb = hFileSb;

        // output forward declaration and comment
        sb.AppendLine($"\n");
        OutputAttachedCommentTrivia(node);
        sb.AppendLine($"// forward declaration");
        sb.AppendLine($"typedef struct {name} {name};");

        foreach (var kid in node.ChildNodes().OfType<StructDeclarationSyntax>())
        {
            Visit(kid);
        }

        foreach (var kid in node.ChildNodes().OfType<ClassDeclarationSyntax>())
        {
            Visit(kid);
        }

        sb = hFileSb;
        foreach (var kid in node.ChildNodes().OfType<DelegateDeclarationSyntax>())
        {
            Visit(kid);
        }
        sb.AppendLine();

        publicSb = hFileSb;
        privateSb = cFileSb;
        CaptureFunctionPrototypes(node);

        sb = hFileSb;
        OutputStruct(node, name);

        sb = cFileSb;
        publicSb = cFileSb;
        privateSb = cFileSb;

        foreach (var kid in node.ChildNodes().OfType<ConstructorDeclarationSyntax>())
        {
            Visit(kid);
        }

        foreach (var kid in node.ChildNodes().OfType<MethodDeclarationSyntax>())
        {
            Visit(kid);
        }
    }

    private void OutputStruct(TypeDeclarationSyntax node, string name, bool outputTypedef = false)
    {
        OutputAttachedCommentTrivia(node);
        if (outputTypedef)
            sb.Append("typedef ");

        sb.Append("struct ");
        sb.AppendTokenAndTrivia(node.Identifier, overrideTokenText: name);
        sb.AppendTokenAndTrivia(node.OpenBraceToken);
        sb.AppendLine(PostProcessor.trimBlankLinesMarker);

        foreach (var kid in node.ChildNodes())
        {
            if (kid is FieldDeclarationSyntax field && !field.IsConst())
                Visit(kid);
        }

        sb.AppendLine(PostProcessor.trimBlankLinesMarker);
        VisitLeadingTrivia(node.CloseBraceToken);
        sb.Append('}');
        if (outputTypedef)
            sb.Append($" {name}");
        sb.Append(';');
        VisitTrailingTrivia(node.CloseBraceToken);
        sb.AppendLine();
    }

    private void CaptureFunctionPrototypes(ClassDeclarationSyntax node)
    {
        renderingPrototypes = true;

        List<SyntaxNode> kids = GetMethodsAndConstructors(node);

        foreach (var kid in kids)
        {
            Visit(kid);
            sb.Append(");\n\n");
        }
        renderingPrototypes = false;
    }

    private List<SyntaxNode> GetMethodsAndConstructors(ClassDeclarationSyntax node)
    {
        List<SyntaxNode> kids = new();
        kids.AddRange(node.ChildNodes().OfType<ConstructorDeclarationSyntax>());
        kids.AddRange(node.ChildNodes().OfType<MethodDeclarationSyntax>().Where(mds => !transpilerHelper.IsGilNoEmit(mds)));
        return kids;
    }

    // delegates are assumed to be method pointers
    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        var symbol = model.GetDeclaredSymbol(node).ThrowIfNull();

        AppendNodeLeadingTrivia(node);
        sb.Append("typedef ");
        Visit(node.ReturnType);
        sb.Append("(*");
        sb.Append(GetCName(symbol));
        sb.Append(')');
        sb.Append("(" + GetCName(symbol.ContainingType) + "* sm)");
        //Visit(node.ParameterList);
        VisitToken(node.SemicolonToken);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        OutputFunctionLeadingTrivia(node);

        sb.Append($"void {node.Identifier.Text}_ctor");

        Visit(node.ParameterList);

        if (!renderingPrototypes)
        {
            var body = node.Body.ThrowIfNull();
            VisitToken(body.OpenBraceToken);
            sb.Append("    memset(sm, 0, sizeof(*sm));\n"); // todo_low - sm should be var so we can use `sm`, `this`, `self`...
            body.VisitChildrenNodesWithWalker(this);
            VisitToken(body.CloseBraceToken);
        }
    }

    private void OutputFunctionLeadingTrivia(SyntaxNode node)
    {
        if (!renderingPrototypes)
        {
            AppendNodeLeadingTrivia(node);
        }
        else
        {
            OutputAttachedCommentTrivia(node);
        }
    }

    private void OutputAttachedCommentTrivia(SyntaxNode node)
    {
        // Only output attached comments. If we find 2 or more end of line trivia without a comment trivia,
        // clear any stored trivia.
        List<SyntaxTrivia> toOutput = new();

        int endOfLineCount = 0;
        foreach (var t in node.GetLeadingTrivia())
        {
            bool isComment = t.IsKind(SyntaxKind.SingleLineCommentTrivia)
                          || t.IsKind(SyntaxKind.MultiLineCommentTrivia); // can also look at others like SingleLineDocumentationCommentTrivia

            if (t.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;
                if (endOfLineCount > 1)
                    toOutput.Clear();
                else if (toOutput.Any()) // append end of line if we already had a comment stored
                    toOutput.Add(t);
            }
            else if (isComment)
            {
                endOfLineCount = 0;
                toOutput.Add(t);
            }
        }

        foreach (var t in toOutput)
        {
            sb.Append(t);
        }
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        sb = node.IsPublic() ? publicSb : privateSb;

        if (transpilerHelper.IsGilNoEmit(node))
        {
            return;
        }

        OutputFunctionLeadingTrivia(node);

        if (!node.IsPublic())
        {
            sb.Append("static ");
        }

        Visit(node.ReturnType);
        VisitToken(node.Identifier);
        Visit(node.ParameterList);

        if (!renderingPrototypes)
            Visit(node.Body);
    }

    public override void VisitBlock(BlockSyntax node)
    {
        if (renderingPrototypes)
            return;

        base.VisitBlock(node);
    }

    // parameters are declared for methods and constructors
    public override void VisitParameterList(ParameterListSyntax node)
    {
        ISymbol? symbol = null;

        if (node.Parent is MethodDeclarationSyntax mds)
        {
            symbol = model.GetDeclaredSymbol(mds).ThrowIfNull();
        }
        else if (node.Parent is ConstructorDeclarationSyntax cds)
        {
            symbol = model.GetDeclaredSymbol(cds).ThrowIfNull();
        }

        var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());

        if (renderingPrototypes)
            list.Remove(node.CloseParenToken);

        if (symbol?.IsStatic == false)
        {
            list.VisitUpTo(node.OpenParenToken, including: true);

            sb.Append(GetCName(symbol.ContainingType) + "* sm");
            if (node.Parameters.Count > 0)
            {
                sb.Append(", ");
            }
        }

        list.VisitRest();
    }

    // arguments are passed to methods/constructors
    public override void VisitArgumentList(ArgumentListSyntax node)
    {
        var invocation = (InvocationExpressionSyntax)node.Parent.ThrowIfNull();
        var iMethodSymbol = (IMethodSymbol)model.GetSymbolInfo(invocation).ThrowIfNull().Symbol.ThrowIfNull();

        if (!iMethodSymbol.IsStatic)
        {
            var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
            list.VisitUpTo(node.OpenParenToken, including: true);

            sb.Append("sm");
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

    public override void VisitParameter(ParameterSyntax node)
    {
        var parameterSymbol = model.GetDeclaredSymbol(node);

        if (parameterSymbol != null && parameterSymbol.Type.IsReferenceType && parameterSymbol.Type.BaseType?.Name != nameof(System.MulticastDelegate))
        {
            Visit(node.Type);
            sb.Append(PostProcessor.trimHorizontalWhiteSpaceMarker); // converts `ROOT_enter(Spec1Sm * sm);` to `ROOT_enter(Spec1Sm* sm);`
            sb.Append("* ");
            VisitToken(node.Identifier);
        }
        else
        {
            base.VisitParameter(node);
        }
    }

    // <Expression> <OperatorToken> <Name>
    // `this.stuff` this == Expression. stuff == Name.
    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        bool done = false;

        if (transpilerHelper.ExpressionIsEnumMember(node.Expression))
        {
            // used for enum access: `MyEnumClass.EnumName`
            Visit(node.Name);
            done = true;
        }
        else if (transpilerHelper.HandleThisMethodAccess(node))
        {
            done = true;
        }
        else
        {
            if (node.IsSimpleMemberAccess())
            {
                done = HandleSimpleMemberAccess(node, done);
            }
        }

        if (!done)
            base.VisitMemberAccessExpression(node);
    }

    private bool HandleSimpleMemberAccess(MemberAccessExpressionSyntax node, bool done)
    {
        bool isPtr = false;

        if (node.Expression is ThisExpressionSyntax tes)
        {
            // `this.stuff` to `sm->stuff`
            VisitLeadingTrivia(tes.Token);
            sb.Append("sm");
            VisitTrailingTrivia(tes.Token);
            isPtr = true;
        }
        // `sm.stuff` to `sm->stuff`
        else if (node.Expression is IdentifierNameSyntax identifierNameSyntax)
        {
            ISymbol? symbol = model.GetSymbolInfo(identifierNameSyntax).Symbol;

            if (symbol is IParameterSymbol parameterSymbol && parameterSymbol.Type.IsReferenceType)
            {
                Visit(identifierNameSyntax);
                isPtr = true;
            }
        }
        else
        {
            Visit(node.Expression);
        }

        if (isPtr)
        {
            sb.Append("->");
            VisitTrailingTrivia(node.OperatorToken);
        }
        else
        {
            VisitToken(node.OperatorToken);
        }

        Visit(node.Name);
        done = true;
        

        return done;
    }

    public override void VisitNullableType(NullableTypeSyntax node)
    {
        // converts `Func? behavior_func` to `Func behavior_func`
        Visit(node.ElementType);
        VisitLeadingTrivia(node.QuestionToken);
        VisitTrailingTrivia(node.QuestionToken);
    }

    public override void VisitToken(SyntaxToken token)
    {
        token.LeadingTrivia.VisitWith(this);

        switch ((SyntaxKind)token.RawKind)
        {
            case SyntaxKind.PublicKeyword:
            case SyntaxKind.EnumKeyword:
            case SyntaxKind.StaticKeyword:
            case SyntaxKind.ReadOnlyKeyword:
            case SyntaxKind.PrivateKeyword:
                return;
        }

        if (token.IsKind(SyntaxKind.ExclamationToken) && token.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression))
        {
            // ignore exclamations like: `this.current_state_exit_handler!();`
        }
        else if (token.IsKind(SyntaxKind.IdentifierToken) && token.Parent is MethodDeclarationSyntax mds)
        {
            sb.Append(GetCName(model.GetDeclaredSymbol(mds).ThrowIfNull()));
        }
        else if (token.IsKind(SyntaxKind.IdentifierToken) && token.Parent is EnumMemberDeclarationSyntax emds)
        {
            sb.Append(GetCName(model.GetDeclaredSymbol(emds).ThrowIfNull()));
        }
        else if (token.IsKind(SyntaxKind.ThisKeyword))
        {
            sb.Append("sm");
        }
        else
        {
            sb.Append(token);
        }

        token.TrailingTrivia.VisitWith(this);
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        sb.Append(trivia);
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



    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var result = node.Identifier.Text;

        switch (result)
        {
            case "Boolean": result = renderConfigC.UseStdBool ? "bool": "int"; break;
            case "SByte": result = "int8_t"; break;
            case "Byte": result = "uint8_t"; break;
            case "Int16": result = "int16_t"; break;
            case "UInt16": result = "uint16_t"; break;
            case "Int32": result = "int32_t"; break;
            case "UInt32": result = "uint32_t"; break;
            case "Int64": result = "int64_t"; break;
            case "UInt64": result = "uint64_t"; break;
            case "Single": result = "float"; break;
            case "Double": result = "double"; break;

            default:
                {
                    SymbolInfo symbol = model.GetSymbolInfo(node);
                    result = GetCName(symbol.Symbol.ThrowIfNull());
                    break;
                }
        }

        node.VisitLeadingTriviaWith(this);
        sb.Append(result);
        node.VisitTrailingTriviaWith(this);
    }

    public override void VisitPredefinedType(PredefinedTypeSyntax node)
    {
        string result = node.Keyword.Text switch
        {
            "void" => "void",
            "bool" => renderConfigC.UseStdBool ? "bool" : "int",
            "sbyte" => "int8_t",
            "byte" => "uint8_t",
            "short" => "int16_t",
            "ushort" => "uint16_t",
            "int" => "int32_t",
            "uint" => "uint32_t",
            "long" => "int64_t",
            "ulong" => "uint64_t",
            "float" => "float",
            "double" => "double",
            "string" => "char const *",
            _ => throw new NotImplementedException(node + ""),
        };

        node.VisitLeadingTriviaWith(this);
        sb.Append(result);
        node.VisitTrailingTriviaWith(this);
    }

    public override void VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        // convert `null` to `NULL`
        if (node.IsKind(SyntaxKind.NullLiteralExpression))
        {
            sb.Append("NULL");
        }
        else
        {
            base.VisitLiteralExpression(node);
        }
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        if (node.Type is ArrayTypeSyntax)
        {
            HandleArrayVarDecl(node);
        }
        else
        {
            base.VisitVariableDeclaration(node);
        }
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        bool done = false;
        bool useDefine = false;
        bool useEnum = true;

        if (node.IsConst())
        {
            if (useDefine)
            {
                done = true;
                AppendNodeLeadingTrivia(node);
                sb.Append("#define ");
                var decl = node.Declaration.Variables.Single();
                sb.Append(GetCName(model.GetDeclaredSymbol(decl).ThrowIfNull()));
                sb.Append(' ');
                Visit(decl.Initializer.ThrowIfNull().Value);
                sb.Append('\n');
            }
            else if (useEnum)
            {
                done = true;
                AppendNodeLeadingTrivia(node);
                sb.Append("enum\n{\n    ");
                Visit(node.Declaration.Variables.Single());
                sb.Append("\n};\n");
            }
        }

        if (!done)
        {
            base.VisitFieldDeclaration(node);
        }
    }

    public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        if (node.FirstAncestorOrSelf<FieldDeclarationSyntax>().IsConst())
        {
            sb.Append(GetCName(model.GetDeclaredSymbol(node).ThrowIfNull()));
            VisitTrailingTrivia(node.Identifier);
        }
        else
        {
            VisitToken(node.Identifier);
        }

        if (node.Initializer?.Value is ObjectCreationExpressionSyntax)
        {
            sb.Append(PostProcessor.trimHorizontalWhiteSpaceMarker);
        }
        else
        {
            Visit(node.Initializer);
        }
    }

    private void HandleArrayVarDecl(VariableDeclarationSyntax node)
    {
        var ats = (ArrayTypeSyntax)node.Type;
        Visit(ats.ElementType);
        sb.Append(' ');

        foreach (VariableDeclaratorSyntax v in node.Variables)
        {
            sb.Append(v.Identifier);

            var rank = v.DescendantNodes().OfType<ArrayRankSpecifierSyntax>().SingleOrDefault();
            if (rank != null)
                Visit(rank);
        }
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        AppendNodeLeadingTrivia(node);
        string name = GetCName(node);

        sb.AppendTokenAndTrivia(node.Identifier, overrideTokenText: customizer.MakeEnumDeclaration(name));
        sb.AppendTokenAndTrivia(node.OpenBraceToken);

        var elements = node.ChildNodesAndTokens().SkipWhile(n => n.IsToken).ToList();
        for (int i = 0; i < elements.Count; i++)
        {
            var kid = elements[i];
            var next = i <= elements.Count - 1 ? elements[i+ 1] : null;
            if (kid.IsNode)
            {
                Visit(kid.AsNode());
            }
            else
            {
                if (kid == node.CloseBraceToken)
                    break;
                if (next == node.CloseBraceToken && kid.IsKind(SyntaxKind.CommaToken))
                {
                    VisitTrailingTrivia(kid.AsToken());
                    break;
                }
                VisitToken(kid.AsToken());
            }
        }

        VisitLeadingTrivia(node.CloseBraceToken);
        sb.Append($"}} {name};");
        VisitTrailingTrivia(node.CloseBraceToken);
    }

    private void AppendNodeLeadingTrivia(SyntaxNode node)
    {
        sb.Append($"{node.GetLeadingTrivia()}");
    }

    private static string MangleTypeSymbolName(string fullyQualifiedName)
    {
        string textName = fullyQualifiedName.Replace(oldChar: '.', newChar: '_');
        return textName;
    }

    private string GetCName(ClassDeclarationSyntax node)
    {
        INamedTypeSymbol symbol = model.GetDeclaredSymbol(node).ThrowIfNull();
        return GetCName(symbol);
    }

    private string GetCName(StructDeclarationSyntax node)
    {
        INamedTypeSymbol symbol = model.GetDeclaredSymbol(node).ThrowIfNull();
        return GetCName(symbol);
    }

    private string GetCName(EnumDeclarationSyntax node)
    {
        INamedTypeSymbol symbol = model.GetDeclaredSymbol(node).ThrowIfNull();
        return GetCName(symbol);
    }

    private string GetCName(ISymbol symbol)
    {
        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (!fieldSymbol.IsStatic && !fieldSymbol.IsConst)
            {
                return fieldSymbol.Name;
            }
        }

        if (symbol.Kind == SymbolKind.Parameter || symbol.Kind == SymbolKind.Local)
        {
            return symbol.Name;
        }

        if (symbol is IMethodSymbol methodSymbol && methodSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return methodSymbol.Name;
        }

        var fqn = transpilerHelper.GetFQN(symbol);
        var name = MangleTypeSymbolName(fqn);
        return name;
    }

    private string GetCName(SymbolInfo symbolInfo)
    {
        return GetCName(symbolInfo.Symbol.ThrowIfNull());
    }
}
