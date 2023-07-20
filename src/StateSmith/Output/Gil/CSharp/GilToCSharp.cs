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
    private readonly RenderConfigVars renderConfig;
    private readonly OutputInfo outputInfo;
    private readonly ISmBaseFileNameProvider stateMachineBaseFileNameProvider;

    public GilToCSharp(OutputInfo outputInfo, RenderConfigCSharpVars renderConfigCSharp, RenderConfigVars renderConfig, ICodeFileWriter codeFileWriter, ISmBaseFileNameProvider stateMachineBaseFileNameProvider)
    {
        this.outputInfo = outputInfo;
        this.renderConfigCSharp = renderConfigCSharp;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.stateMachineBaseFileNameProvider = stateMachineBaseFileNameProvider;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        CSharpGilVisitor cSharpGilVisitor = new(gilCode, fileSb, renderConfigCSharp, renderConfig);
        cSharpGilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.outputDirectory}{stateMachineBaseFileNameProvider.Get()}.cs", code: fileSb.ToString());
    }
}
