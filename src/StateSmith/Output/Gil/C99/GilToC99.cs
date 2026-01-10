#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Output.Algos.Balanced1;

namespace StateSmith.Output.Gil.C99;

public class GilToC99 : IGilTranspiler
{
    public readonly StringBuilder hFileSb = new();
    public readonly StringBuilder cFileSb = new();
    public readonly RenderConfigCVars renderConfigC;
    public readonly RenderConfigBaseVars renderConfig;

    private readonly ICodeFileWriter codeFileWriter;
    private readonly IOutputInfo outputInfo;
    private readonly IGilToC99Customizer cCustomizer;
    private readonly RoslynCompiler roslynCompiler;
    private readonly NameMangler nameMangler;

    public GilToC99(
        IOutputInfo outputInfo,
        IGilToC99Customizer cCustomizer,
        ICodeFileWriter codeFileWriter,
        RenderConfigBaseVars renderConfig,
        RenderConfigCVars renderConfigC,
        RoslynCompiler roslynCompiler,
        NameMangler nameMangler)
    {
        this.renderConfigC = renderConfigC;
        this.outputInfo = outputInfo;
        this.cCustomizer = cCustomizer;
        this.codeFileWriter = codeFileWriter;
        this.renderConfig = renderConfig;
        this.roslynCompiler = roslynCompiler;
        this.nameMangler = nameMangler;
    }

    public void TranspileAndOutputCode(string programText)
    {
        cCustomizer.Setup();

        //File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.SmName}.gil.cs", programText);

        roslynCompiler.Compile(programText, out CompilationUnitSyntax root, out SemanticModel model);

        C99GenVisitor visitor = new(model, hFileSb, cFileSb, renderConfig, renderConfigC, cCustomizer, outputInfo, nameMangler);

        visitor.Visit(root);

        PostProcessor.PostProcess(hFileSb);
        PostProcessor.PostProcess(cFileSb);

        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{cCustomizer.MakeHFileName()}", code: hFileSb.ToString());
        codeFileWriter.WriteFile($"{outputInfo.OutputDirectory}{cCustomizer.MakeCFileName()}", code: cFileSb.ToString());
    }
}
