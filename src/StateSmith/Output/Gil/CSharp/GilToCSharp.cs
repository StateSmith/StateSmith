using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Output.Algos.Balanced1;    // todo need a generic way of getting file name. RenderConfig?

#nullable enable

namespace StateSmith.Output.Gil.CSharp;

public class GilToCSharp : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigCSharpVars renderConfigCSharp;
    private readonly RenderConfigVars renderConfig;
    private readonly OutputInfo outputInfo;
    private readonly NameMangler nameMangler;

    public GilToCSharp(OutputInfo outputInfo, NameMangler nameMangler, RenderConfigCSharpVars renderConfigCSharp, RenderConfigVars renderConfig, ICodeFileWriter codeFileWriter)
    {
        this.outputInfo = outputInfo;
        this.nameMangler = nameMangler;
        this.renderConfigCSharp = renderConfigCSharp;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        CSharpGilVisitor cSharpGilVisitor = new(gilCode, fileSb, renderConfigCSharp, renderConfig);
        cSharpGilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.outputDirectory}{nameMangler.BaseFileName}.cs", code: fileSb.ToString());
    }
}
