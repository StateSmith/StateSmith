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
        if (!GilHelper.IsGilNoEmit(node))
        {
            base.VisitMethodDeclaration(node);
        }
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // append class code after open brace token
        void action() => sb.AppendLineIfNotBlank(renderConfigCSharp.ClassCode);
        this.VisitNodeRunActionAfterToken(node, node.OpenBraceToken, action);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        bool done = false;
        // FIXME move to GilAlgoHelper
        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilHelper.GilNoEmitEchoStringBoolFuncName)
            {
                done = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(argumentSyntax.ToFullString());
                unescaped = unescaped[1..^1]; // range operator
                sb.Append(unescaped); // FIXME: this may not do everything we need. We need inverse of https://stackoverflow.com/a/58825732/7331858 
            }
        }
        
        if(!done)
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
