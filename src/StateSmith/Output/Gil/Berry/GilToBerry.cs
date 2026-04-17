#nullable enable

using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.Berry;

public class GilToBerry : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();
    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigBerryVars renderConfigBerry;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IOutputInfo outputInfo;
    private readonly RoslynCompiler roslynCompiler;
    private readonly CodeStyleSettings codeStyleSettings;

    public GilToBerry(IOutputInfo outputInfo,
        RenderConfigBerryVars renderConfigBerry,
        RenderConfigBaseVars renderConfig,
        ICodeFileWriter codeFileWriter,
        RoslynCompiler roslynCompiler,
        CodeStyleSettings codeStyleSettings)
    {
        this.outputInfo = outputInfo;
        this.renderConfigBerry = renderConfigBerry;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.roslynCompiler = roslynCompiler;
        this.codeStyleSettings = codeStyleSettings;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        BerryGilVisitor gilVisitor = new(gilCode, fileSb, renderConfigBerry, renderConfig, roslynCompiler, codeStyleSettings, moduleName: outputInfo.BaseFileName);
        gilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.be", code: fileSb.ToString());
    }
}
