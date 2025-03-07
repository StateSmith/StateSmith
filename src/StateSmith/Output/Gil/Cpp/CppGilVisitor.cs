#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Common;
using System.Linq;
using System;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.Cpp;

public class CppGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder hFileSb = new();
    public readonly StringBuilder cFileSb = new();

    private bool renderingHeader = false;
    private int classDepth = 0;
    private StringBuilder privateSb = new();
    private StringBuilder publicSb = new();
    private StringBuilder sb;
    private readonly CppGilHelpers cppObjs;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly IncludeGuardProvider includeGuardProvider;

    private SemanticModel model;

    // Not yet supported. See https://github.com/StateSmith/StateSmith/issues/411
    private bool useStaticDelegates = true;    // could make this a user accessible setting

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder? _methodPtrFinder;
    private SyntaxToken? tokenToSkip;

    /// <summary>Only valid if <see cref="useStaticDelegates"/> true.</summary>
    private MethodPtrFinder MethodPtrFinder => _methodPtrFinder.ThrowIfNull();

    RenderConfigCppVars renderConfigCpp => cppObjs.renderConfigCpp;
    RenderConfigBaseVars renderConfig => cppObjs.renderConfig;

    public CppGilVisitor(string gilCode, CppGilHelpers cppGilHelpers) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = cFileSb;
        this.cppObjs = cppGilHelpers;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, cppObjs.roslynCompiler);
        model = transpilerHelper.model;
        includeGuardProvider = new IncludeGuardProvider(renderConfigCpp.IncludeGuardLabel, cppObjs.outputInfo);
    }

    public void Process()
    {
        if (useStaticDelegates)
        {
            _methodPtrFinder = new(transpilerHelper.root, transpilerHelper.model);
            MethodPtrFinder.Find();
        }

        OutputHFileTopSections();
        OutputCFileTopSections();

        renderingHeader = true;
        sb = hFileSb;
        this.DefaultVisit(transpilerHelper.root);

        renderingHeader = false;
        sb = new StringBuilder(); // temp so that we can deindent
        this.DefaultVisit(transpilerHelper.root);
        cFileSb.Append(StringUtils.DeIndent(sb.ToString()));
        sb = cFileSb;

        OutputFileBottomSections();
    }

    private void OutputHFileTopSections()
    {
        sb = hFileSb;
        transpilerHelper.PreProcess();
        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        sb.AppendLineIfNotBlank(renderConfigCpp.HFileTop);

        includeGuardProvider.OutputIncludeGuardTop(sb);
        sb.AppendLineIfNotBlank(renderConfigCpp.HFileTopPostIncludeGuard);

        sb.AppendLine("#include <stdint.h>");
        sb.AppendLineIfNotBlank(renderConfigCpp.HFileIncludes);
        sb.AppendLine();
    }

    public string MakeHFileName()
    {
        return $"{cppObjs.outputInfo.BaseFileName}{renderConfigCpp.HFileExtension}";
    }

    public string MakeCFileName()
    {
        return $"{cppObjs.outputInfo.BaseFileName}{renderConfigCpp.CFileExtension}";
    }

    private void OutputCFileTopSections()
    {
        sb = cFileSb;
        transpilerHelper.PreProcess();
        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        sb.AppendLineIfNotBlank(renderConfigCpp.CFileTop);
        sb.AppendLine($"#include \"{MakeHFileName()}\"");
        sb.AppendLine("#include <stdbool.h> // required for `consume_event` flag");
        sb.AppendLine("#include <string.h> // for memset");
        sb.AppendLineIfNotBlank(renderConfigCpp.CFileIncludes);
        sb.AppendLine();
    }

    private void OutputFileBottomSections()
    {
        hFileSb.AppendLineIfNotBlank(renderConfigCpp.HFileBottomPreIncludeGuard);
        includeGuardProvider.OutputIncludeGuardBottom(hFileSb);
        hFileSb.AppendLineIfNotBlank(renderConfigCpp.HFileBottom);

        cFileSb.AppendLineIfNotBlank(renderConfigCpp.CFileBottom);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        classDepth++;

        var nameSpace = renderConfigCpp.NameSpace.Trim();
        if (classDepth == 1 && nameSpace.Length > 0)
        {
            sb.AppendLine("namespace " + renderConfigCpp.NameSpace);
            sb.AppendLine("{");
            sb.AppendLine(PostProcessor.trimBlankLinesMarker);
        }

        if (!renderingHeader)
        {
            foreach (var method in node.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                Visit(method);
            }
        }
        else
        {
            var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());

            if (classDepth == 1)
            {
                // skip public on top level classes
                list.VisitUpToThenSkip(node.Modifiers.Single(), outputLeadingTrivia: true);

                // output template declaration if base class is not specified
                // TODO consolidate with MaybeoutputBaseList?
                var baseList = renderConfigCpp.BaseClassCode.Trim();
                if (baseList.Length == 0)
                {
                    sb.AppendLine("template <typename Base>");
                }
            }
            list.VisitUpTo(node.Identifier, including: true);
            StringUtils.EraseTrailingWhitespace(sb);

            if (classDepth == 1)    // fix for https://github.com/StateSmith/StateSmith/issues/427
            {
                MaybeOutputBaseList();
            }
            VisitTrailingTrivia(node.Identifier);

            list.VisitUpTo(node.OpenBraceToken, including: true);

            // sort remaining members by public/private

            var members = node.ChildNodes().OfType<MemberDeclarationSyntax>();
            var publicMembers = members.Where(f => f.IsPublic());
            var privateMembers = members.Where(f => !f.IsPublic());

            var classLevelIndent = StringUtils.FindIndent(node.OpenBraceToken.LeadingTrivia.ToFullString());

            if (publicMembers.Any())
            {
                sb.Append($"{classLevelIndent}public:\n");
                foreach (var item in publicMembers)
                {
                    Visit(item);
                }
            }

            if (classDepth == 1 && !String.IsNullOrWhiteSpace(renderConfigCpp.ClassCode))
            {
                sb.AppendLine("");
                string classCode = renderConfigCpp.ClassCode;
                sb.Append(classCode);
                sb.AppendLine();
            }

            if (privateMembers.Any())
            {
                sb.AppendLine($"{classLevelIndent}");
                sb.AppendLine($"{classLevelIndent}");
                sb.AppendLine($"{classLevelIndent}// ################################### PRIVATE MEMBERS ###################################");
                sb.AppendLine($"{classLevelIndent}private:");
                foreach (var item in privateMembers)
                {
                    Visit(item);
                }
            }

            VisitToken(node.CloseBraceToken);
            StringUtils.EraseTrailingWhitespace(sb);
            sb.Append(";\n");
        }

        if (classDepth == 1 && nameSpace.Length > 0)
        {
            sb.AppendLine("}");
        }

        classDepth--;
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        WalkableChildSyntaxList list = new(this, node.ChildNodesAndTokens());
        list.VisitUpTo(node.Identifier);
        sb.Append("class ");
        list.VisitNext(); // identifier
        StringUtils.EraseTrailingWhitespace(sb);

        // if the enum has less than 256 members, use uint8_t to save RAM. Otherwise leave it up to compiler.
        var enumMemberCount = node.ChildNodes().Count();
        if (enumMemberCount < byte.MaxValue)
        {
            sb.Append(": uint8_t");
        }

        VisitTrailingTrivia(node.Identifier);
        list.VisitRest();

        StringUtils.EraseTrailingWhitespace(sb);
        sb.Append(";\n");
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialFieldDeclarations(node, sb))
            return;

        if (node.IsConst())
        {
            VisitToken(node.GetFirstToken()); // should be 'public' or 'private'
            sb.Append("enum\n    {\n        ");
            Visit(node.Declaration.Variables.Single());
            sb.Append("\n    };\n");    // todolow - get proper indentation setting
            return;
        }

        base.VisitFieldDeclaration(node);
    }

    public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
    {
        if (node.Value is ObjectCreationExpressionSyntax oces)
        {
            sb.Append("{}");
            return;
        }

        base.VisitEqualsValueClause(node);
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
        if (transpilerHelper.HandleThisMethodAccess(node))
            return;

        var expressionSymbolInfo = model.GetSymbolInfo(node.Expression);

        Visit(node.Expression);

        bool needsColonColon = false;

        if (expressionSymbolInfo.Symbol is ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                needsColonColon = true;
            }
        }

        if (needsColonColon)
        {
            sb.Append("::");
        }
        else
        {
            if (node.Expression is ThisExpressionSyntax)
            {
                sb.Append("->");
            }
            else
            {
                sb.Append(".");
            }
        }

        Visit(node.Name);
        //base.VisitMemberAccessExpression(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (transpilerHelper.IsGilData(node))
            return;

        MaybeOutputStaticDelegate(node);

        var list = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());

        if (renderingHeader)
        {
            list.VisitUpTo(node.Body.ThrowIfNull());
            StringUtils.EraseTrailingWhitespace(sb);
            sb.Append(";\n");
        }
        else
        {
            list.VisitUpTo(node.Identifier);
            IMethodSymbol symbol = model.GetDeclaredSymbol(node).ThrowIfNull();

            sb.Append(symbol.ContainingSymbol.Name + "::");
            list.VisitRest();
        }
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

    private void MaybeOutputBaseList()
    {
        var baseList = renderConfigCpp.BaseClassCode.Trim();
        if (baseList.Length > 0)
            sb.Append(" : " + baseList);
        else
            sb.Append(" : Base");
    }

    // to ignore GIL attributes
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        sb.Append(node.ToFullString());
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

    public override void VisitPredefinedType(PredefinedTypeSyntax node)
    {
        string result = node.Keyword.Text switch
        {
            "void" => "void",
            "bool" => "bool",
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

    // kinda like: https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp/Syntax/InternalSyntax/SyntaxToken.cs,516c0eb61810c3ef,references
    public override void VisitToken(SyntaxToken token)
    {
        if (token == tokenToSkip)
        {
            tokenToSkip = null;
            return;
        }

        string tokenText = token.Text;
        bool visitTrailingTrivia = true;
        bool visitLeadingTrivia = true;

        if (renderingHeader)
        {
            switch ((SyntaxKind)token.RawKind)
            {
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.PrivateKeyword:
                    visitTrailingTrivia = false;
                    tokenText = "";
                    break;
            }
        }
        else
        {
            switch ((SyntaxKind)token.RawKind)
            {
                case SyntaxKind.PrivateKeyword:
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.StaticKeyword:
                    visitTrailingTrivia = false;
                    tokenText = "";
                    break;
            }
        }

        if (visitLeadingTrivia)
        {
            this.VisitLeadingTrivia(token);
        }

        sb.Append(tokenText);

        if (visitTrailingTrivia)
        {
            this.VisitTrailingTrivia(token);
        }
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
