using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Linq;
using StateSmith.Output.UserConfig;
using System;

#nullable enable

namespace StateSmith.Output.Gil.CSharp;

internal class CSharpGilVisitor : CSharpSyntaxWalker
{
    public readonly StringBuilder sb;
    private readonly OutputInfo outputInfo;
    private readonly RenderConfigCSharpVars renderConfigCSharp;
    private readonly RenderConfigVars renderConfig;

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

        gilCode = CSharpSyntaxTree.ParseText(gilCode).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
        GilHelper.Compile(gilCode, out CompilationUnitSyntax root, out _, outputInfo);

        this.Visit(root);

        FormatOutput();
    }

    private void FormatOutput()
    {
        var outputCode = sb.ToString();
        outputCode = CSharpSyntaxTree.ParseText(outputCode).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
        sb.Clear();
        sb.Append(outputCode);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (GilHelper.IsGilNoEmit(node))
            return;

        bool isAddressableFunction = false;
        foreach (var attributeList in node.AttributeLists)
        {
            if (attributeList.Attributes.Any(attr => attr.Name.ToString() == GilHelper.GilAddessableFunction))
            {
                isAddressableFunction = true;
                break;
            }
        }

        if (isAddressableFunction)
        {
            //            File.Append($"private static readonly Func {mangler.SmFuncTriggerHandler(state, eventName)} = ({mangler.SmStructName} sm) =>");

        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (GilHelper.HandleSpecialGilEmitClasses(node, this)) return;

        // append class code after open brace token
        void action() => sb.AppendLineIfNotBlank(renderConfigCSharp.ClassCode);
        this.VisitNodeRunActionAfterToken(node, node.OpenBraceToken, action);
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
