using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.IO;
using StateSmith.Output.UserConfig;
using StateSmith.Output.C99BalancedCoder1;

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

        GilHelper.Compile(programText, out CompilationUnitSyntax root, out SemanticModel model);

        CGenVisitor visitor = new(model, hFileSb, cFileSb, renderConfigC, cNameMangler.HFileName);

        visitor.Visit(root);

        PostProcessor.PostProcess(hFileSb);
        PostProcessor.PostProcess(cFileSb);

        File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.HFileName}", hFileSb.ToString());
        File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.CFileName}", cFileSb.ToString());
    }
}
