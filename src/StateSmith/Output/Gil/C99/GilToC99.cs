using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Xml.Linq;
using System.IO;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.Output.C99BalancedCoder1;
using Antlr4.Runtime.Tree;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Output.Gil.C99;

public class GilToC99 : IGilTranspiler
{
    public readonly StringBuilder hFileSb = new();
    public readonly StringBuilder cFileSb = new();
    public readonly RenderConfigCVars renderConfigC;
    private readonly OutputInfo outputInfo;
    private readonly CNameMangler cNameMangler;

    public GilToC99(RenderConfigCVars renderConfigC, OutputInfo outputInfo, CNameMangler cNameMangler)
    {
        this.renderConfigC = renderConfigC;
        this.outputInfo = outputInfo;
        this.cNameMangler = cNameMangler;
    }


    public void TranspileAndOutputCode(string programText)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.SmName}.gil.cs", programText);

        Compile(programText, out CompilationUnitSyntax root, out SemanticModel model);

        CGenVisitor visitor = new(model, hFileSb, cFileSb, renderConfigC, cNameMangler.HFileName);

        visitor.Visit(root);

        PostProcessor.PostProcess(hFileSb);
        PostProcessor.PostProcess(cFileSb);

        File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.HFileName}", hFileSb.ToString());
        File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.CFileName}", cFileSb.ToString());
    }

    private void Compile(string programText, out CompilationUnitSyntax root, out SemanticModel model)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
        root = tree.GetCompilationUnitRoot();
        ThrowOnError(tree.GetDiagnostics(), programText);

        var compilation = CSharpCompilation.Create("GilCompilation")
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        model = compilation.GetSemanticModel(tree);
        ThrowOnError(model.GetDiagnostics(), programText);
    }

    private void ThrowOnError(IEnumerable<Diagnostic> enumerable, string programText)
    {
        var errors = enumerable.Where(d => d.Severity == DiagnosticSeverity.Error);

        var message = "";

        foreach (var error in errors)
        {
            message += error.ToString() + "\n";
        }

        if (message.Length > 0)
        {
            //File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.SmName}.gil.cs", programText);
            throw new GilCompError(message);
        }
    }
}
