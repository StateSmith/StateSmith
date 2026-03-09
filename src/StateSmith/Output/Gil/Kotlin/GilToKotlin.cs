#nullable enable

using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.Kotlin;

public class GilToKotlin : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigKotlinVars renderConfigKotlin;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IOutputInfo outputInfo;
    private readonly RoslynCompiler roslynCompiler;

    public GilToKotlin(IOutputInfo outputInfo, RenderConfigKotlinVars renderConfigKotlin, RenderConfigBaseVars renderConfig, ICodeFileWriter codeFileWriter, RoslynCompiler roslynCompiler)
    {
        this.outputInfo = outputInfo;
        this.renderConfigKotlin = renderConfigKotlin;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.roslynCompiler = roslynCompiler;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        KotlinGilVisitor gilVisitor = new(gilCode, fileSb, renderConfigKotlin, renderConfig, roslynCompiler);
        gilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.kt", code: fileSb.ToString());
    }
}
