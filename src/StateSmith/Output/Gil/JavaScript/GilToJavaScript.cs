using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Output.Algos.Balanced1;    // todo need a generic way of getting file name. RenderConfig?
using StateSmith.Runner;

#nullable enable

namespace StateSmith.Output.Gil.JavaScript;

public class GilToJavaScript : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigVars renderConfig;
    private readonly RenderConfigJavaScriptVars renderConfigJavaScript;
    private readonly IOutputInfo outputInfo;

    public GilToJavaScript(IOutputInfo outputInfo, RenderConfigVars renderConfig, RenderConfigJavaScriptVars renderConfigJavaScript, ICodeFileWriter codeFileWriter)
    {
        this.outputInfo = outputInfo;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.renderConfigJavaScript = renderConfigJavaScript;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        JavaScriptGilVisitor visitor = new(gilCode, fileSb, renderConfig, renderConfigJavaScript);
        visitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.js", code: fileSb.ToString());
    }

}
