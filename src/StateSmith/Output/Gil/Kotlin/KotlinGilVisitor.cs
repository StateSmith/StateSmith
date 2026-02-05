using System;
#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System.Collections.Generic;

namespace StateSmith.Output.Gil.Kotlin;

/// <summary>
/// NOTE! This is mostly a copy of CSharp transpiler with some Kotlin specific changes.
/// </summary>
public class KotlinGilVisitor : CSharpSyntaxWalker
{
    private readonly StringBuilder mainSb;
    private readonly RenderConfigKotlinVars renderConfigKotlin;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;

    private SemanticModel model;
    private StringBuilder sb;
    private StringBuilder? companionSb;


    private SyntaxToken? tokenToSkip;

    public KotlinGilVisitor(string gilCode, StringBuilder sb, RenderConfigKotlinVars renderConfigKotlin, RenderConfigBaseVars renderConfig, RoslynCompiler roslynCompiler) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = mainSb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigKotlin = renderConfigKotlin;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, roslynCompiler);
        model = transpilerHelper.model;
    }

    public void Process()
    {
        transpilerHelper.PreProcess();

        sb.AppendLineIfNotBlank(renderConfig.FileTop);

        var package = renderConfigKotlin.Package.Trim();
        if (package.Length > 0)
        {
            sb.AppendLine("package " + renderConfigKotlin.Package);
            sb.AppendLine();
        }

        sb.AppendLineIfNotBlank(renderConfigKotlin.Imports, optionalTrailer: "\n");

        this.Visit(transpilerHelper.root);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        var parentSb = this.companionSb;
        var companionSb = new StringBuilder();
        this.companionSb = companionSb;

        var iterableChildSyntaxList = new WalkableChildSyntaxList(this, node.ChildNodesAndTokens());
        
        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);

        iterableChildSyntaxList.SkipUpTo(SyntaxKind.ClassKeyword);

        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }
        else if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            sb.Append("protected ");
        } 
        else if (node.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            sb.Append("internal ");
        }

        iterableChildSyntaxList.VisitUpTo(node.Identifier);

        // handle identifier specially so that it doesn't put base list on newline
        iterableChildSyntaxList.Remove(node.Identifier);
        sb.Append(node.Identifier.Text);
        MaybeOutputBaseList();
        VisitTrailingTrivia(node.Identifier);

        iterableChildSyntaxList.VisitUpTo(node.OpenBraceToken, including: true);
        sb.AppendLineIfNotBlank(renderConfigKotlin.ClassCode);  // append class code after open brace token

        iterableChildSyntaxList.VisitUpTo(node.CloseBraceToken);
        if (companionSb.Length > 0)
        {
            sb.Append(indent);
            sb.AppendLine("companion object {");
            sb.Append(companionSb.ToString());
            sb.Append(indent);
            sb.AppendLine("}");
        }

        this.companionSb = parentSb;

        iterableChildSyntaxList.VisitRest();
    }

    private void MaybeOutputBaseList()
    {
        var extends = renderConfigKotlin.Extends.Trim();
        var implements = renderConfigKotlin.Implements.Trim();
        if (extends.Length > 0 && implements.Length > 0)
        {
            sb.Append(" : ");
            sb.Append(extends);
            sb.Append("(), ");
            sb.Append(implements);
        } 
        else if (extends.Length > 0)
        {
            sb.Append(" : ");
            sb.Append(extends);
            sb.Append("()");
        }
        else if (implements.Length > 0)
        {
            sb.Append(" : ");
            sb.Append(implements);
        }
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }
        else if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            sb.Append("protected ");
        } 
        else if (node.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            sb.Append("internal ");
        }

        sb.Append("enum class ");        
        sb.Append(node.Identifier.Text);

        VisitToken(node.OpenBraceToken);

        for (var i = 0; i < node.Members.Count; i++)
        {
            Visit(node.Members[i]);
            if (i == node.Members.Count - 1)            
                sb.AppendLine();          
            else  
                sb.AppendLine(", ");
        }

        VisitToken(node.CloseBraceToken);
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append(node.Identifier.Text);
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialFieldDeclarations(node, sb))
            return;

        if (node.Modifiers.Any(SyntaxKind.StaticKeyword) && companionSb != null)
        {
            sb = companionSb;
        }

        VisitLeadingTrivia(node.GetFirstToken());

        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }
        else if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            sb.Append("protected ");
        } 
        else if (node.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            sb.Append("internal ");
        }

        Visit(node.Declaration);
        sb = mainSb;

        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        var variable = node.Variables.Single();
        if (node.Parent is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ConstKeyword))
            sb.Append("val ");
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
            if (fieldSymbol != null && fieldSymbol.Type.NullableAnnotation != NullableAnnotation.Annotated)
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
            var defaultMember = transpilerHelper.GetDefaultEnumMember(typeSymbol);
            return $"{typeSymbol.Name}.{defaultMember.Name}";
        }
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean => "false",
            SpecialType.System_Char => "'\\0'",
            _ => "0"
        };
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        // TODO: include constructor in the class declaration using semantic model (when needed)
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
        if (transpilerHelper.IsGilData(node))
            return;

        if (node.Modifiers.Any(SyntaxKind.StaticKeyword) && companionSb != null)
        {
            sb = companionSb;
        }

        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);

        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("private ");
        }
        else if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            sb.Append("protected ");
        } 
        else if (node.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            sb.Append("internal ");
        }

        sb.Append("fun ");

        VisitToken(node.Identifier);

        VisitParameterList(node.ParameterList);

        if (node.ReturnType != null && (node.ReturnType as PredefinedTypeSyntax)?.Keyword.RawKind != (int)SyntaxKind.VoidKeyword)
        {
            sb.Append(indent);
            sb.Append(" : ");
            Visit(node.ReturnType);
        }

        VisitBlock(node.Body!);
        sb = mainSb;
    }

    public override void VisitParameter(ParameterSyntax node)
    {
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
                    sb.AppendLine("try {");
                    VisitStatements(block.Statements);
                    sb.Append(indent);
                    sb.Append("} finally {}");
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

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        Visit(node.Type);
        //Visit(node.ArgumentList);  // we need to update how `VisitArgumentList()` works
        sb.Append("()");
    }

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialExpressionStatements(node, sb))
            return;

        base.VisitExpressionStatement(node);
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    { 
        VisitLeadingTrivia(node.GetFirstToken());
        string indent = StringUtils.FindLastIndent(sb);
        sb.Append("when (");
        Visit(node.Expression);
        sb.AppendLine(")");
        VisitToken(node.OpenBraceToken);
        foreach (var section in node.Sections) {
            Visit(section);
        }
        
        var typeInfo = model.GetTypeInfo(node.Expression);
        var memberNames = typeInfo.Type == null || typeInfo.Type.TypeKind != TypeKind.Enum ? null : transpilerHelper.GetEnumMembers(typeInfo.Type).Select(m => $"{typeInfo.Type.Name}.{m.Name}").ToHashSet();
        var labelNames = node.Sections.SelectMany(s => s.Labels).OfType<CaseSwitchLabelSyntax>().Select(l => l.Value.ToFullString()).ToHashSet();
        var isDefaultUnnecessary = memberNames != null && labelNames.IsSupersetOf(memberNames);

        if (!node.Sections.SelectMany(s => s.Labels).Any(l => l.Keyword.RawKind == (int)SyntaxKind.DefaultKeyword) && !isDefaultUnnecessary)
        {
            sb.Append(indent);
            sb.AppendLine("else -> {}");
        }

        VisitToken(node.CloseBraceToken);
    }

    public override void VisitSwitchSection(SwitchSectionSyntax node)
    {        
        var switchStatement = (SwitchStatementSyntax) node.Parent;
        var typeInfo = model.GetTypeInfo(switchStatement.Expression);
        var memberNames = typeInfo.Type == null || typeInfo.Type.TypeKind != TypeKind.Enum ? null : transpilerHelper.GetEnumMembers(typeInfo.Type).Select(m => $"{typeInfo.Type.Name}.{m.Name}").ToHashSet();
        var labelNames = switchStatement.Sections.SelectMany(s => s.Labels).OfType<CaseSwitchLabelSyntax>().Select(l => l.Value.ToFullString()).ToHashSet();
        var isDefaultUnnecessary = memberNames != null && labelNames.IsSupersetOf(memberNames);

        var labels = isDefaultUnnecessary ? node.Labels.Where(l => l is not DefaultSwitchLabelSyntax).ToList() : node.Labels.ToList();

        if (labels.Count == 0)
        {
            return;
        }

        string indent = StringUtils.FindLastIndent(sb);
        Visit(labels[0]);
        foreach (var label in labels.Skip(1))
        {
            if (isDefaultUnnecessary && label is DefaultSwitchLabelSyntax)
                continue;

            sb.Append(", ");
            Visit(label);
        }

        var breakIndex = node.Statements.TakeWhile((s) => s is not BreakStatementSyntax).Count();
        var returnIndex = node.Statements.TakeWhile((s) => s is not ReturnStatementSyntax).Count();
        if (breakIndex == node.Statements.Count && returnIndex == node.Statements.Count)
        {
            throw new Exception("Fallthrough not supported!");
        }
        else if (breakIndex < returnIndex)
        {
            if (breakIndex != 1)
            {
                sb.Append(indent);
                sb.Append("{ ");
            }
            foreach (var statement in node.Statements.Take(breakIndex))
            {
                Visit(statement);
            }
            if (breakIndex != 1)
            {
                sb.Append("} ");
            }
            VisitTrailingTrivia(node.Statements.Last().GetLastToken());
        }
        else
        {
            if (returnIndex != 0)
            {
                sb.Append(indent);
                sb.Append("{ ");
            }
            foreach (var statement in node.Statements.Take(returnIndex + 1))
            {
                Visit(statement);
            }
            if (returnIndex != 0)
            {
                sb.Append("} ");
            }
        }
    }

    public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        Visit(node.Value);
        sb.Append(" -> ");
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("else -> ");
        VisitTrailingTrivia(node.GetLastToken());
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
            case SyntaxKind.SemicolonToken: tokenText = ""; break;
            case SyntaxKind.StructKeyword: tokenText = "class"; break;
            case SyntaxKind.VirtualKeyword: tokenText = "open"; break;
            case SyntaxKind.IntKeyword: tokenText = "Int"; break;
            case SyntaxKind.StringKeyword: tokenText = "String"; break;
            case SyntaxKind.BoolKeyword: tokenText = "Boolean"; break;
            case SyntaxKind.FloatKeyword: tokenText = "Float"; break;
            case SyntaxKind.DoubleKeyword: tokenText = "Double"; break;
        }

        sb.Append(tokenText);

        token.TrailingTrivia.VisitWith(this);
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        sb.Append(trivia.ToString());
    }
}
