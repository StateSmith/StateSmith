#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.C99;

public class GilToC99 : IGilTranspiler
{
    public readonly StringBuilder hFileSb = new();
    public readonly StringBuilder cFileSb = new();
    public readonly RenderConfigCVars renderConfigC;
    public readonly RenderConfigVars renderConfig;

    private readonly ICodeFileWriter codeFileWriter;
    private readonly IOutputInfo outputInfo;
    private readonly IGilToC99Customizer cCustomizer;

    public GilToC99(IOutputInfo outputInfo, IGilToC99Customizer cCustomizer, ICodeFileWriter codeFileWriter, RenderConfigVars renderConfig, RenderConfigCVars renderConfigC)
    {
        this.renderConfigC = renderConfigC;
        this.outputInfo = outputInfo;
        this.cCustomizer = cCustomizer;
        this.codeFileWriter = codeFileWriter;
        this.renderConfig = renderConfig;
    }

    public void TranspileAndOutputCode(string programText)
    {
        cCustomizer.Setup();

        //File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.SmName}.gil.cs", programText);

        GilTranspilerHelper.Compile(programText, out CompilationUnitSyntax root, out SemanticModel model);

        C99GenVisitor visitor = new(model, hFileSb, cFileSb, renderConfig, renderConfigC, cCustomizer);

        visitor.Visit(root);

        PostProcessor.PostProcess(hFileSb);
        PostProcessor.PostProcess(cFileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{cCustomizer.MakeHFileName()}", code: hFileSb.ToString());
        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{cCustomizer.MakeCFileName()}", code: cFileSb.ToString());
    }
}
