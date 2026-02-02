#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Common;
using StateSmith.Output.Gil;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateSmith.Output.Gil.Berry;

/// <summary>
/// Placeholder visitor that currently mirrors the Python transpiler. Will be iterated into real Berry output.
/// </summary>
public class BerryGilVisitor : CSharpSyntaxWalker
{
    private StringBuilder sb;
    private readonly RenderConfigBerryVars renderConfigBerry;
    private readonly GilTranspilerHelper transpilerHelper;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly CodeStyleSettings codeStyleSettings;
    private readonly SemanticModel model;
    private readonly string moduleName;
    private readonly List<string> topLevelClassNames = new();
    private readonly List<string> classNameStack = new();
    private readonly List<(string alias, string expression)> moduleAdditionalExports = new();
    private string Indent => codeStyleSettings.Indent1;
    private SyntaxToken? tokenToSkip;
    private SyntaxToken? tokenWithSuppressedLeadingTrivia;
    private int indentLevel;

    public BerryGilVisitor(string gilCode,
        StringBuilder sb,
        RenderConfigBerryVars renderConfigBerry,
        RenderConfigBaseVars renderConfig,
        RoslynCompiler roslynCompiler,
        CodeStyleSettings codeStyleSettings,
        string? moduleName = null) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.renderConfig = renderConfig;
        this.renderConfigBerry = renderConfigBerry;
        this.codeStyleSettings = codeStyleSettings;
        this.moduleName = moduleName ?? string.Empty;
        transpilerHelper = GilTranspilerHelper.Create(this, gilCode, roslynCompiler);
        transpilerHelper.EchoStringTransformer = TransformUnsupportedSyntax;
        model = transpilerHelper.model;
    }

    private string TransformUnsupportedSyntax(string text)
    {
        text = ConvertCStyleCommentsToBerry(text);
        text = RemoveStatementSemicolonsInsideBackticks(text);
        return text;
    }

    public void Process()
    {
        transpilerHelper.PreProcess();

        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        sb.AppendLineIfNotBlank(renderConfigBerry.Imports, optionalTrailer: "\n");

        this.Visit(transpilerHelper.root);
        EmitModuleExports();
    }

    private string CurrentIndent()
    {
        return CurrentIndent(indentLevel);
    }

    private string CurrentIndent(int level)
    {
        if (level <= 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        for (int i = 0; i < level; i++)
        {
            builder.Append(Indent);
        }

        return builder.ToString();
    }

    private void WriteIndent()
    {
        WriteIndentLevel(indentLevel);
    }

    private void WriteIndentLevel(int level)
    {
        if (level <= 0)
        {
            return;
        }

        sb.Append(CurrentIndent(level));
    }

    private void SuppressLeadingTriviaOnce(SyntaxToken token)
    {
        if (token == default)
        {
            return;
        }

        tokenWithSuppressedLeadingTrivia = token;
    }

    private void WriteLine(string text = "")
    {
        WriteIndent();
        sb.AppendLine(text);
    }

    private int CalculateExpectedIndentLevel(SyntaxNode node)
    {
        int blockDepth = 0;
        foreach (var ancestor in node.Ancestors())
        {
            if (ancestor is BlockSyntax block && BlockContributesIndent(block))
            {
                blockDepth++;
            }
        }

        return classNameStack.Count + blockDepth;
    }

    private int GetEffectiveIndentLevel(SyntaxNode node)
    {
        int expected = CalculateExpectedIndentLevel(node);
        return Math.Max(indentLevel, expected);
    }

    private void WriteRawStatement(string text, int indentLevelOverride)
    {
        if (string.IsNullOrEmpty(text))
        {
            sb.AppendLine();
            return;
        }

        string normalized = text.Replace("\r\n", "\n").TrimEnd('\r', '\n');
        if (normalized.Length == 0)
        {
            sb.AppendLine();
            return;
        }

        string[] lines = normalized.Split('\n');
        int minIndent = int.MaxValue;

        foreach (string rawLine in lines)
        {
            string trimmedLine = rawLine.TrimEnd('\r');
            if (trimmedLine.Trim().Length == 0)
            {
                continue;
            }

            minIndent = Math.Min(minIndent, CountLeadingWhitespace(trimmedLine));
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');
            if (line.Trim().Length == 0)
            {
                sb.AppendLine();
                continue;
            }

            if (minIndent > 0)
            {
                int remove = Math.Min(minIndent, CountLeadingWhitespace(line));
                line = line.Substring(remove);
            }

            WriteIndentLevel(indentLevelOverride);
            sb.AppendLine(line);
        }
    }

    private static int CountLeadingWhitespace(string text)
    {
        int count = 0;
        foreach (char c in text)
        {
            if (c == ' ' || c == '\t')
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    private void VisitBlockStatements(BlockSyntax node)
    {
        indentLevel++;

        VisitTriviaList(node.OpenBraceToken.TrailingTrivia);

        if (node.Statements.Count == 0)
        {
            WriteLine("");
        }
        else
        {
            foreach (var statement in node.Statements)
            {
                Visit(statement);
            }
        }

        VisitTriviaList(node.CloseBraceToken.LeadingTrivia);

        indentLevel--;

        VisitTriviaList(node.CloseBraceToken.TrailingTrivia);
    }

    private void VisitTriviaList(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            VisitTrivia(trivia);
        }
    }

    private bool LastCharIsNewline()
    {
        if (sb.Length == 0)
        {
            return true;
        }

        char last = sb[sb.Length - 1];
        return last == '\n' || last == '\r';
    }

    private void VisitStatementBody(StatementSyntax statement)
    {
        if (statement is BlockSyntax block)
        {
            VisitBlockStatements(block);
        }
        else
        {
            indentLevel++;
            Visit(statement);
            indentLevel--;
        }
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (transpilerHelper.HandleSpecialGilEmitClasses(node)) return;

        bool isNested = EnterType(node.Identifier.Text);
        try
        {

            VisitLeadingTrivia(node.GetFirstToken());
            WriteIndent();
            if (isNested)
            {
                sb.Append("static ");
            }
            sb.Append("class ");
            sb.Append(node.Identifier.Text);

            if (!isNested && !string.IsNullOrWhiteSpace(renderConfigBerry.Extends))
            {
                sb.Append(" : ");
                sb.Append(renderConfigBerry.Extends);
            }

            sb.AppendLine();

            indentLevel++;

            if (!isNested && !string.IsNullOrWhiteSpace(renderConfigBerry.ClassCode))
            {
                foreach (var line in renderConfigBerry.ClassCode.Split('\n'))
                {
                    var trimmed = line.TrimEnd('\r');
                    if (trimmed.Length != 0)
                    {
                        WriteLine(trimmed);
                    }
                }
            }

            EmitFieldDeclarations(node);

            bool hasConstructor = node.ChildNodes().OfType<ConstructorDeclarationSyntax>().Any();
            if (!hasConstructor)
            {
                WriteLine("def init()");
                indentLevel++;
                EmitFieldInitializers(node);
                indentLevel--;
                WriteLine("end");
            }

            foreach (var member in node.Members)
            {
                if (member is FieldDeclarationSyntax)
                {
                    continue;
                }

                Visit(member);
            }

            indentLevel--;
            WriteLine("end");
        }
        finally
        {
            ExitType();
        }
    }

    private bool EnterType(string name)
    {
        classNameStack.Add(name);

        bool isNested = classNameStack.Count > 1;

        if (!isNested)
        {
            topLevelClassNames.Add(name);
        }
        else if (classNameStack.Count == 2 && !string.IsNullOrWhiteSpace(moduleName))
        {
            string topLevelClassName = classNameStack[0];
            if (string.Equals(topLevelClassName, moduleName, StringComparison.Ordinal))
            {
                moduleAdditionalExports.Add((name, topLevelClassName + "." + name));
            }
        }

        return isNested;
    }

    private void ExitType()
    {
        classNameStack.RemoveAt(classNameStack.Count - 1);
    }

    private void EmitModuleExports()
    {
        if (string.IsNullOrWhiteSpace(moduleName) || topLevelClassNames.Count == 0)
        {
            return;
        }

        if (!LastCharIsNewline())
        {
            sb.AppendLine();
        }

        string moduleVarName = moduleName + "Module";
        sb.Append("var ");
        sb.Append(moduleVarName);
        sb.Append(" = module(\"");
        sb.Append(moduleName);
        sb.AppendLine("\")");

        var exportedNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var className in topLevelClassNames)
        {
            if (!exportedNames.Add(className))
            {
                continue;
            }

            EmitModuleAssignment(moduleVarName, className, className);
        }

        foreach (var (alias, expression) in moduleAdditionalExports)
        {
            if (!exportedNames.Add(alias))
            {
                continue;
            }

            EmitModuleAssignment(moduleVarName, alias, expression);
        }

        sb.Append("return ");
        sb.AppendLine(moduleVarName);

        void EmitModuleAssignment(string moduleVar, string name, string expression)
        {
            sb.Append(moduleVar);
            sb.Append('.');
            sb.Append(name);
            sb.Append(" = ");
            sb.AppendLine(expression);
        }
    }

    private bool EmitFieldInitializers(ClassDeclarationSyntax node)
    {
        bool wrote = false;

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            int before = sb.Length;
            Visit(field);
            if (sb.Length > before)
            {
                wrote = true;
            }
        }

        return wrote;
    }

    private void EmitFieldDeclarations(ClassDeclarationSyntax node)
    {
        HashSet<string> declaredGilFields = new(StringComparer.Ordinal);

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (TryEmitGilFieldDeclaration(field, declaredGilFields))
            {
                continue;
            }

            foreach (var variable in field.Declaration.Variables)
            {
                if (transpilerHelper.IsGilData(variable.Identifier.Text))
                {
                    continue;
                }

                WriteIndent();
                if (field.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    sb.Append("static ");
                }

                sb.Append("var ");
                sb.Append(variable.Identifier.Text);
                sb.AppendLine();
            }
        }
    }

    private bool TryEmitGilFieldDeclaration(FieldDeclarationSyntax field, HashSet<string> declaredGilFields)
    {
        if (field.Declaration.Variables.Count != 1)
        {
            return false;
        }

        var variable = field.Declaration.Variables[0];
        if (!transpilerHelper.IsGilData(variable.Identifier.Text))
        {
            return false;
        }

        if (!variable.Identifier.Text.StartsWith(GilCreationHelper.GilFieldName_EchoString, StringComparison.Ordinal))
        {
            return false;
        }

        string originalCode = GilTranspilerHelper.GetOriginalCodeFromGilFieldEcho(field).Trim();
        if (!TryExtractAssignmentTarget(originalCode, out string fieldName))
        {
            return false;
        }

        if (!declaredGilFields.Add(fieldName))
        {
            return true;
        }

        WriteIndent();
        if (field.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            sb.Append("static ");
        }

        sb.Append("var ");
        sb.Append(fieldName);
        sb.AppendLine();

        return true;
    }

    private static bool TryExtractAssignmentTarget(string originalCode, out string fieldName)
    {
        fieldName = string.Empty;
        if (string.IsNullOrWhiteSpace(originalCode))
        {
            return false;
        }

        const string selfPrefix = "self.";
        var trimmed = originalCode.Trim();
        if (!trimmed.StartsWith(selfPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        int startIndex = selfPrefix.Length;
        int endIndex = trimmed.IndexOfAny(new[] { ' ', '\t', '=', '(', ')', ';' }, startIndex);
        if (endIndex == -1)
        {
            endIndex = trimmed.Length;
        }

        string candidate = trimmed[startIndex..endIndex].Trim();
        if (candidate.Length == 0)
        {
            return false;
        }

        fieldName = candidate;
        return true;
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        EnterType(node.Identifier.Text);
        try
        {
            VisitLeadingTrivia(node.GetFirstToken());
            WriteIndent();
            sb.Append("static ");
            sb.Append("class ");
            sb.Append(node.Identifier.Text);
            sb.AppendLine();

            indentLevel++;
            int implicitValue = 0;

            foreach (var member in node.Members)
            {
                WriteIndent();
                sb.Append("static var ");
                sb.Append(member.Identifier.Text);
                sb.Append(" = ");

                string valueText;
                if (member.EqualsValue != null)
                {
                    valueText = CaptureStringBuf(() => Visit(member.EqualsValue!.Value));
                    if (int.TryParse(valueText, out var parsed))
                    {
                        implicitValue = parsed + 1;
                    }
                }
                else
                {
                    valueText = implicitValue.ToString();
                    implicitValue++;
                }

                sb.AppendLine(valueText);
            }

            indentLevel--;
            WriteLine("end");
        }
        finally
        {
            ExitType();
        }
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        WriteIndent();
        sb.Append("if ");
        Visit(node.Condition);
        sb.AppendLine();

        VisitStatementBody(node.Statement);
        VisitElseChain(node.Else);
        WriteLine("end");
    }

    private void VisitElseChain(ElseClauseSyntax? elseClause)
    {
        if (elseClause == null)
        {
            return;
        }

        if (elseClause.Statement is IfStatementSyntax nested)
        {
            WriteIndent();
            sb.Append("elif ");
            Visit(nested.Condition);
            VisitStatementBody(nested.Statement);
            VisitElseChain(nested.Else);
        }
        else
        {
            WriteIndent();
            sb.AppendLine("else");
            VisitStatementBody(elseClause.Statement);
        }
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        WriteIndent();
        sb.Append("while ");
        Visit(node.Condition);
        VisitStatementBody(node.Statement);
        WriteLine("end");
    }

    public override void VisitBlock(BlockSyntax node)
    {
        if (!BlockContributesIndent(node))
        {
            VisitTriviaList(node.OpenBraceToken.TrailingTrivia);
            foreach (var statement in node.Statements)
            {
                Visit(statement);
            }
            VisitTriviaList(node.CloseBraceToken.LeadingTrivia);
            VisitTriviaList(node.CloseBraceToken.TrailingTrivia);
            return;
        }

        VisitBlockStatements(node);
    }

    private static bool BlockContributesIndent(BlockSyntax block)
    {
        return block.Parent is not BlockSyntax;
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (transpilerHelper.HandleGilSpecialFieldDeclarations(node, sb, CurrentIndent()))
            return;

        var variable = node.Declaration.Variables.Single();

        WriteIndent();
        sb.Append("self.");
        sb.Append(variable.Identifier.Text);
        sb.Append(" = ");

        if (variable.Initializer != null)
        {
            Visit(variable.Initializer.Value);
        }
        else
        {
            sb.Append("nil");
        }

        sb.AppendLine();
    }

    private string CaptureStringBuf(Action action)
    {
        var originalSb = sb;
        sb = new StringBuilder();
        action();
        var captured = sb.ToString();
        sb = originalSb;
        return captured;
    }

    public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        sb.Append(node.Identifier.Text);
        sb.Append(" = ");

        if (node.Initializer == null)
        {
            sb.Append("nil");
        }
        else
        {
            Visit(node.Initializer.Value);
        }
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        sb.Append("self.");
        Visit(node.Type);
        sb.Append("()");
        VisitTrailingTrivia(node.GetLastToken());
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());

        if (node.ParameterList.ChildNodes().Any())
        {
            throw new NotImplementedException("Constructors with parameters are not yet supported.");
        }

        WriteLine("def init()");
        indentLevel++;

        var classNode = node.Ancestors().OfType<ClassDeclarationSyntax>().First();
        bool wroteFields = EmitFieldInitializers(classNode);

        if (!(node.Body!.Statements.Count == 0 && !wroteFields))
        {
            foreach (var statement in node.Body!.Statements)
            {
                Visit(statement);
            }
        }

        indentLevel--;
        WriteLine("end");
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var result = node.Identifier.Text;

        SymbolInfo symbol = model.GetSymbolInfo(node);
        var resolvedSymbol = symbol.Symbol;

        bool shouldPrefixSelf = resolvedSymbol is IFieldSymbol fieldSymbol
            && !fieldSymbol.IsStatic
            && !fieldSymbol.IsConst
            && node.Parent is not MemberAccessExpressionSyntax;

        if (resolvedSymbol != null)
        {
            result = GetBerryName(resolvedSymbol);
        }

        node.VisitLeadingTriviaWith(this);

        if (shouldPrefixSelf)
        {
            sb.Append("self.");
        }

        sb.Append(result);
        node.VisitTrailingTriviaWith(this);
    }

    private string GetBerryName(ISymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Enum)
        {
            return transpilerHelper.GetFQN(symbol);
        }

        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (!fieldSymbol.IsStatic && !fieldSymbol.IsConst)
            {
                var result = "";
                if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
                    result = "_";

                result += fieldSymbol.Name;
                return result;
            }
        }

        if (symbol is IMethodSymbol methodSymbol)
        {
            var result = "";
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                result = "_";

            result += methodSymbol.Name;
            return result;
        }

        return symbol.Name;
    }

    public override void VisitNullableType(NullableTypeSyntax node)
    {
        Visit(node.ElementType);
        sb.Append(' ');
    }

    public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
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

        bool isStatic = node.Modifiers.Any(SyntaxKind.StaticKeyword);

        VisitLeadingTrivia(node.GetFirstToken());
        WriteIndent();

        if (isStatic)
        {
            sb.Append("static ");
        }

        sb.Append("def ");

        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            sb.Append("_");
        }

        sb.Append(node.Identifier.Text);
        sb.Append("(");

        var parameters = node.ParameterList.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(parameters[i].Identifier.Text);
        }

        sb.AppendLine(")");

        VisitBlockStatements(node.Body!);
        WriteLine("end");
    }

    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        if (!HasAnyUsefulStatements(node))
        {
            return;
        }

        VisitLeadingTrivia(node.GetFirstToken());
        string switchValue = CaptureStringBuf(() => Visit(node.Expression));
        bool anyWritten = false;

        foreach (var section in node.Sections)
        {
            var caseLabels = section.Labels.OfType<CaseSwitchLabelSyntax>().ToList();
            bool hasDefault = section.Labels.OfType<DefaultSwitchLabelSyntax>().Any();

            if (caseLabels.Count == 0 && !hasDefault)
            {
                continue;
            }

            if (hasDefault && !anyWritten)
            {
                WriteIndent();
                sb.AppendLine("if true");
            }
            else if (hasDefault)
            {
                WriteIndent();
                sb.AppendLine("else");
            }
            else
            {
                WriteIndent();
                sb.Append(anyWritten ? "elif " : "if ");

                var comparisons = new List<string>();
                foreach (var label in caseLabels)
                {
                    var labelText = CaptureStringBuf(() => Visit(label.Value));
                    comparisons.Add($"{switchValue} == {labelText}");
                }

                sb.Append(string.Join(" || ", comparisons));
                sb.AppendLine();
            }

            VisitSwitchSectionBody(section);
            anyWritten = true;
        }

        if (anyWritten)
        {
            WriteLine("end");
        }
    }

    private static bool HasAnyUsefulStatements(SyntaxNode node)
    {
        if (node is SwitchStatementSyntax switchNode)
        {
            return switchNode.Sections.SelectMany(section => section.Statements).Any(statement => statement is not BreakStatementSyntax);
        }

        if (node is SwitchSectionSyntax sectionNode)
        {
            return sectionNode.Statements.Any(statement => statement is not BreakStatementSyntax);
        }

        return node.DescendantNodes().Any(descendant => descendant is ExpressionStatementSyntax || descendant is ReturnStatementSyntax);
    }

    private void VisitSwitchSectionBody(SwitchSectionSyntax section)
    {
        indentLevel++;

        var statements = section.Statements.Where(s => s is not BreakStatementSyntax).ToList();
        if (statements.Count != 0)
        {
            foreach (var statement in statements)
            {
                Visit(statement);
            }
        }

        indentLevel--;
    }

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        int indentForStatement = GetEffectiveIndentLevel(node);
        if (transpilerHelper.TryGetGilSpecialExpressionText(node, out var rawText))
        {
            VisitLeadingTrivia(node.GetFirstToken());
            WriteRawStatement(rawText, indentForStatement);
            return;
        }

        VisitLeadingTrivia(node.GetFirstToken());
        WriteIndentLevel(indentForStatement);
        SuppressLeadingTriviaOnce(node.Expression.GetFirstToken());
        Visit(node.Expression);
        node.GetLastToken().TrailingTrivia.VisitWith(this);
        sb.AppendLine();
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        WriteIndent();
        sb.Append("return");

        if (node.Expression != null)
        {
            sb.Append(' ');
            Visit(node.Expression);
        }

        node.GetLastToken().TrailingTrivia.VisitWith(this);
        sb.AppendLine();
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        bool done = false;

        done |= transpilerHelper.HandleGilSpecialInvocations(node, sb);
        done |= transpilerHelper.HandleGilUnusedVarSpecialInvocation(node, argument =>
        {
            ExpressionStatementSyntax expressionParent = (ExpressionStatementSyntax)node.Parent!;
            tokenToSkip = expressionParent.SemicolonToken;
        });

        if (!done)
        {
            base.VisitInvocationExpression(node);
        }
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        foreach (var variable in node.Variables)
        {
            WriteIndent();
            Visit(variable);
            sb.AppendLine();
        }
    }

    public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        if (node.IsKind(SyntaxKind.LogicalNotExpression))
        {
            sb.Append("! ");
            Visit(node.Operand);
        }
        else
        {
            base.VisitPrefixUnaryExpression(node);
        }
    }

    public override void VisitToken(SyntaxToken token)
    {
        if (token == tokenToSkip)
        {
            tokenToSkip = null;
            return;
        }

        bool suppressLeadingTrivia = tokenWithSuppressedLeadingTrivia.HasValue && token == tokenWithSuppressedLeadingTrivia.Value;
        if (suppressLeadingTrivia)
        {
            tokenWithSuppressedLeadingTrivia = null;
        }
        else
        {
            token.LeadingTrivia.VisitWith(this);
        }

        string tokenText = token.Text;

        switch ((SyntaxKind)token.RawKind)
        {
            case SyntaxKind.ConstKeyword:
            case SyntaxKind.OpenBraceToken:
            case SyntaxKind.CloseBraceToken:
            case SyntaxKind.SemicolonToken:
            case SyntaxKind.ClassKeyword:
            case SyntaxKind.ColonToken: tokenText = ""; break;
            case SyntaxKind.TrueKeyword: tokenText = "true"; break;
            case SyntaxKind.FalseKeyword: tokenText = "false"; break;
            case SyntaxKind.NullKeyword: tokenText = "nil"; break;
            case SyntaxKind.AmpersandAmpersandToken: tokenText = " and "; break;
            case SyntaxKind.BarBarToken: tokenText = " or "; break;
            case SyntaxKind.ThisKeyword: tokenText = "self"; break;
        }

        if (!(token.IsKind(SyntaxKind.ExclamationToken) && token.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression)))
        {
            sb.Append(tokenText);
        }

        token.TrailingTrivia.VisitWith(this);
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.WhitespaceTrivia:
                if (!LastCharIsNewline())
                {
                    sb.Append(' ');
                }
                return;

            case SyntaxKind.EndOfLineTrivia:
                sb.Append(codeStyleSettings.Newline);
                return;

            case SyntaxKind.SingleLineCommentTrivia:
                WriteCommentLine(trivia.ToString().TrimStart('/'));
                return;

            case SyntaxKind.MultiLineCommentTrivia:
                EmitMultiLineComment(trivia.ToString());
                return;

            default:
                sb.Append(trivia.ToString());
                return;
        }
    }

    private void EmitMultiLineComment(string rawComment)
    {
        var content = rawComment.Trim();
        if (content.StartsWith("/*"))
        {
            content = content[2..];
        }
        if (content.EndsWith("*/"))
        {
            content = content[..^2];
        }

        var lines = content.Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                WriteCommentLine(string.Empty);
            }
            else
            {
                WriteCommentLine(trimmed);
            }
        }
    }

    private void WriteCommentLine(string commentText)
    {
        var trimmed = commentText.TrimStart();
        WriteIndent();
        sb.Append('#');
        if (trimmed.Length > 0)
        {
            sb.Append(' ');
            sb.Append(trimmed);
        }
        sb.AppendLine();
    }

    private static string ConvertCStyleCommentsToBerry(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf("/*", StringComparison.Ordinal) < 0)
        {
            return text;
        }

        var builder = new StringBuilder();
        bool insideSingleQuotes = false;
        bool insideDoubleQuotes = false;

        for (int i = 0; i < text.Length;)
        {
            if (!insideSingleQuotes && !insideDoubleQuotes && i + 1 < text.Length && text[i] == '/' && text[i + 1] == '*')
            {
                int commentStart = i;
                i += 2;
                var commentContent = new StringBuilder();

                while (i < text.Length && !(text[i] == '*' && i + 1 < text.Length && text[i + 1] == '/'))
                {
                    commentContent.Append(text[i]);
                    i++;
                }

                if (i >= text.Length)
                {
                    builder.Append(text.AsSpan(commentStart));
                    return builder.ToString();
                }

                i += 2; // skip closing */

                AppendBerryComment(builder, commentContent.ToString());
                continue;
            }

            char current = text[i];
            builder.Append(current);

            if (current == '"' && !insideSingleQuotes && !IsEscaped(text, i))
            {
                insideDoubleQuotes = !insideDoubleQuotes;
            }
            else if (current == '\'' && !insideDoubleQuotes && !IsEscaped(text, i))
            {
                insideSingleQuotes = !insideSingleQuotes;
            }

            i++;
        }

        return builder.ToString();
    }

    private static string RemoveStatementSemicolonsInsideBackticks(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.IndexOf(';') < 0 || text.IndexOf('`') < 0)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        int cursor = 0;

        while (cursor < text.Length)
        {
            int backtickStart = text.IndexOf('`', cursor);
            if (backtickStart < 0)
            {
                builder.Append(text.AsSpan(cursor));
                break;
            }

            builder.Append(text.AsSpan(cursor, backtickStart - cursor + 1));
            int backtickEnd = text.IndexOf('`', backtickStart + 1);
            if (backtickEnd < 0)
            {
                builder.Append(text.AsSpan(backtickStart + 1));
                break;
            }

            string codeFragment = text.Substring(backtickStart + 1, backtickEnd - backtickStart - 1);
            builder.Append(RemoveTrailingSemicolons(codeFragment));
            builder.Append('`');
            cursor = backtickEnd + 1;
        }

        return builder.ToString();
    }

    private static string RemoveTrailingSemicolons(string codeFragment)
    {
        if (string.IsNullOrEmpty(codeFragment) || codeFragment.IndexOf(';') < 0)
        {
            return codeFragment;
        }

        var builder = new StringBuilder(codeFragment.Length);
        int i = 0;

        while (i < codeFragment.Length)
        {
            char current = codeFragment[i];

            if (current == ';')
            {
                int j = i + 1;
                while (j < codeFragment.Length && char.IsWhiteSpace(codeFragment[j]))
                {
                    j++;
                }

                bool remove = j >= codeFragment.Length || codeFragment[j] == '}' || codeFragment[j] == ']';
                if (remove)
                {
                    i++;
                    continue;
                }
            }

            builder.Append(current);
            i++;
        }

        return builder.ToString();
    }

    private static void AppendBerryComment(StringBuilder builder, string rawContent)
    {
        var (lineHasCode, indent) = AnalyzeCurrentLine(builder);

        if (lineHasCode)
        {
            builder.Append('\n');
        }
        else
        {
            TrimTrailingWhitespace(builder);
        }

        var normalizedContent = rawContent.Replace("\r\n", "\n");
        var lines = normalizedContent.Split('\n');

        if (lines.Length == 0)
        {
            lines = new[] { string.Empty };
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            builder.Append(indent);
            builder.Append('#');
            if (trimmed.Length > 0)
            {
                builder.Append(' ');
                builder.Append(trimmed);
            }
            builder.Append('\n');
        }
    }

    private static (bool lineHasCode, string indent) AnalyzeCurrentLine(StringBuilder builder)
    {
        if (builder.Length == 0)
        {
            return (false, string.Empty);
        }

        int cursor = builder.Length - 1;
        while (cursor >= 0 && builder[cursor] != '\n')
        {
            cursor--;
        }

        int lineStart = cursor + 1;
        var indentBuilder = new StringBuilder();
        int indentCursor = lineStart;
        while (indentCursor < builder.Length && (builder[indentCursor] == ' ' || builder[indentCursor] == '\t'))
        {
            indentBuilder.Append(builder[indentCursor]);
            indentCursor++;
        }

        bool lineHasCode = false;
        for (int i = indentCursor; i < builder.Length; i++)
        {
            if (builder[i] != ' ' && builder[i] != '\t')
            {
                lineHasCode = true;
                break;
            }
        }

        return (lineHasCode, indentBuilder.ToString());
    }

    private static void TrimTrailingWhitespace(StringBuilder builder)
    {
        while (builder.Length > 0)
        {
            char c = builder[builder.Length - 1];
            if (c == ' ' || c == '\t')
            {
                builder.Length--;
            }
            else
            {
                break;
            }
        }
    }

    private static bool IsEscaped(string text, int index)
    {
        int backslashCount = 0;
        int cursor = index - 1;

        while (cursor >= 0 && text[cursor] == '\\')
        {
            backslashCount++;
            cursor--;
        }

        return backslashCount % 2 == 1;
    }
}
