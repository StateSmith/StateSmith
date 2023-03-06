using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.IO;
using StateSmith.Output.UserConfig;

#nullable enable

namespace StateSmith.Output.Gil.C99;

public class GilToC99 : IGilTranspiler
{
    public readonly StringBuilder hFileSb = new();
    public readonly StringBuilder cFileSb = new();
    public readonly RenderConfigCVars renderConfigC;

    private readonly OutputInfo outputInfo;
    private readonly IGilToC99Customizer cCustomizer;

    public GilToC99(RenderConfigCVars renderConfigC, OutputInfo outputInfo, IGilToC99Customizer cCustomizer)
    {
        this.renderConfigC = renderConfigC;
        this.outputInfo = outputInfo;
        this.cCustomizer = cCustomizer;
    }

    public void TranspileAndOutputCode(string programText)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.SmName}.gil.cs", programText);

        GilHelper.Compile(programText, out CompilationUnitSyntax root, out SemanticModel model);

        C99GenVisitor visitor = new(model, hFileSb, cFileSb, renderConfigC, cCustomizer);

        visitor.Visit(root);

        PostProcessor.PostProcess(hFileSb);
        PostProcessor.PostProcess(cFileSb);

        File.WriteAllText($"{outputInfo.outputDirectory}{cCustomizer.MakeHFileName()}", hFileSb.ToString());
        File.WriteAllText($"{outputInfo.outputDirectory}{cCustomizer.MakeCFileName()}", cFileSb.ToString());
    }
}
