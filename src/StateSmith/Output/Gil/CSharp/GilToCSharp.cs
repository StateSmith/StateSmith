using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Output.Algos.Balanced1;    // todo need a generic way of getting file name. RenderConfig?
using StateSmith.Runner;

#nullable enable

namespace StateSmith.Output.Gil.CSharp;

public class GilToCSharp : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigCSharpVars renderConfigCSharp;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IOutputInfo outputInfo;
    private readonly RoslynCompiler roslynCompiler;

    public GilToCSharp(IOutputInfo outputInfo, RenderConfigCSharpVars renderConfigCSharp, RenderConfigBaseVars renderConfig, ICodeFileWriter codeFileWriter, RoslynCompiler roslynCompiler)
    {
        this.outputInfo = outputInfo;
        this.renderConfigCSharp = renderConfigCSharp;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.roslynCompiler = roslynCompiler;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        CSharpGilVisitor cSharpGilVisitor = new(gilCode, fileSb, renderConfigCSharp, renderConfig, roslynCompiler);
        cSharpGilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.cs", code: fileSb.ToString());
    }
}
