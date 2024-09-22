#nullable enable

using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.TypeScript;

public class GilToTypeScript : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigTypeScriptVars renderConfigTypeScript;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IOutputInfo outputInfo;
    private readonly RoslynCompiler roslynCompiler;
    private readonly CodeStyleSettings codeStyleSettings;

    public GilToTypeScript(IOutputInfo outputInfo, RenderConfigTypeScriptVars renderConfigTypeScript, RenderConfigBaseVars renderConfig, ICodeFileWriter codeFileWriter, RoslynCompiler roslynCompiler, CodeStyleSettings codeStyleSettings)
    {
        this.outputInfo = outputInfo;
        this.renderConfigTypeScript = renderConfigTypeScript;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.roslynCompiler = roslynCompiler;
        this.codeStyleSettings = codeStyleSettings;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        TypeScriptGilVisitor gilVisitor = new(gilCode, fileSb, renderConfigTypeScript, renderConfig, roslynCompiler, codeStyleSettings);
        gilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.ts", code: fileSb.ToString());
    }
}
