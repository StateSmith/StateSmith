#nullable enable

using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Gil.Swift;

public class GilToSwift : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();

    private readonly ICodeFileWriter codeFileWriter;
    private readonly RenderConfigSwiftVars renderConfigSwift;
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IOutputInfo outputInfo;
    private readonly RoslynCompiler roslynCompiler;
    private readonly CodeStyleSettings codeStyleSettings;

    public GilToSwift(IOutputInfo outputInfo, RenderConfigSwiftVars renderConfigSwift, RenderConfigBaseVars renderConfig, ICodeFileWriter codeFileWriter, RoslynCompiler roslynCompiler, CodeStyleSettings settings)
    {
        this.outputInfo = outputInfo;
        this.renderConfigSwift = renderConfigSwift;
        this.renderConfig = renderConfig;
        this.codeFileWriter = codeFileWriter;
        this.roslynCompiler = roslynCompiler;
        this.codeStyleSettings = settings;
    }

    public void TranspileAndOutputCode(string gilCode)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        SwiftGilVisitor gilVisitor = new(gilCode, fileSb, renderConfigSwift, renderConfig, roslynCompiler, codeStyleSettings);
        gilVisitor.Process();

        PostProcessor.PostProcess(fileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.py", code: fileSb.ToString());
    }
}
