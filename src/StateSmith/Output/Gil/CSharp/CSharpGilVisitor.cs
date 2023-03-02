using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using Microsoft.CodeAnalysis.Formatting;

#nullable enable

namespace StateSmith.Output.Gil.CSharp;

internal class CSharpGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder sb;
    private readonly OutputInfo outputInfo;
    private readonly RenderConfigCSharpVars renderConfigCSharp;
    private readonly RenderConfigVars renderConfig;

    private SemanticModel SemanticModel => _semanticModel.ThrowIfNull();
    private SemanticModel? _semanticModel;

    public CSharpGilVisitor(StringBuilder sb, OutputInfo outputInfo, RenderConfigCSharpVars renderConfigCSharp, RenderConfigVars renderConfig) : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        this.sb = sb;
        this.outputInfo = outputInfo;
        this.renderConfig = renderConfig;
        this.renderConfigCSharp = renderConfigCSharp;
    }

    public void Process()
    {
        // get input gil code and then clear string buffer to hold new result
        string gilCode = sb.ToString();
        sb.Clear();

        sb.AppendLineIfNotBlank(renderConfig.FileTop);
        sb.AppendLine($"#nullable enable"); // todo_low - add config option
        sb.AppendLineIfNotBlank(renderConfigCSharp.Usings);

        GilHelper.Compile(gilCode, out CompilationUnitSyntax root, out _semanticModel, outputInfo);

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

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (GilHelper.IsGilNoEmit(node))
            return;

        var addressableFunc = GilHelper.GetAddresssableFunctionInfo(node, SemanticModel);

        if (addressableFunc != null)
        {
            OutputMethodAsStaticLambda(node, addressableFunc);
        }
        else
        {
            base.VisitMethodDeclaration(node);
        }
    }

    /// <summary>
    /// Why do this? See https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support#function-pointers
    /// </summary>
    /// <param name="node"></param>
    /// <param name="addressableFunc"></param>
    private void OutputMethodAsStaticLambda(MethodDeclarationSyntax node, GilHelper.AddressableFunctionInfo addressableFunc)
    {
        VisitLeadingTrivia(node.GetFirstToken());
        foreach (var m in node.Modifiers)
            VisitToken(m);

        sb.Append("readonly ");
        sb.Append(addressableFunc.DelegateSymbol.Name);
        sb.Append(' ');
        sb.Append(node.Identifier);
        sb.Append($" = ");

        Visit(addressableFunc.ParameterListSyntax);

        sb.Append($" => ");
        node.Body.ThrowIfNull().VisitChildNodesAndTokens(this, toSkip: node.Body.CloseBraceToken);
        sb.AppendTokenAndTrivia(node.Body.CloseBraceToken, overrideTokenText: "};");
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (GilHelper.HandleSpecialGilEmitClasses(node, this)) return;

        // append class code after open brace token
        void action() => sb.AppendLineIfNotBlank(renderConfigCSharp.ClassCode);
        node.VisitChildNodesAndTokens(this, node.OpenBraceToken, action);
    }

    // to ignore GIL attributes
    public override void VisitAttributeList(AttributeListSyntax node)
    {
        VisitLeadingTrivia(node.GetFirstToken());
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (GilHelper.HandleGilSpecialInvocations(node, sb))
            return;

        base.VisitInvocationExpression(node);
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
