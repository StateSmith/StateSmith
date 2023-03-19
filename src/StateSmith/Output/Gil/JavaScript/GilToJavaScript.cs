using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Output.Algos.Balanced1;    // todo need a generic way of getting file name. RenderConfig?

#nullable enable

namespace StateSmith.Output.Gil.JavaScript;

public class GilToJavaScript : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigVars renderConfig;
    private readonly RenderConfigJavaScriptVars renderConfigJavaScript;
    private readonly OutputInfo outputInfo;
    private readonly NameMangler nameMangler;

    public GilToJavaScript(OutputInfo outputInfo, NameMangler nameMangler, RenderConfigVars renderConfig, RenderConfigJavaScriptVars renderConfigJavaScript, ICodeFileWriter codeFileWriter)
    {
        this.outputInfo = outputInfo;
        this.nameMangler = nameMangler;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.renderConfigJavaScript = renderConfigJavaScript;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        JavaScriptGilVisitor visitor = new(fileSb, renderConfig, renderConfigJavaScript);
        visitor.Process(gilCode);

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.outputDirectory}{nameMangler.BaseFileName}.js", code: fileSb.ToString());
    }

}
