#nullable enable

using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.Java;

public class GilToJava : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigJavaVars renderConfigJava;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IOutputInfo outputInfo;
    private readonly RoslynCompiler roslynCompiler;

    public GilToJava(IOutputInfo outputInfo, RenderConfigJavaVars renderConfigJava, RenderConfigBaseVars renderConfig, ICodeFileWriter codeFileWriter, RoslynCompiler roslynCompiler)
    {
        this.outputInfo = outputInfo;
        this.renderConfigJava = renderConfigJava;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.roslynCompiler = roslynCompiler;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        JavaGilVisitor gilVisitor = new(gilCode, fileSb, renderConfigJava, renderConfig, roslynCompiler);
        gilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.java", code: fileSb.ToString());
    }
}
