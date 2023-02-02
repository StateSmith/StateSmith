using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System.IO;

#nullable enable

namespace StateSmith.Output;

internal class CodeGenRunner
{
    readonly DynamicVarsResolver varsResolver;
    readonly CodeGenContext codeGenContext;
    readonly IRenderConfigC renderConfig;
    readonly ConfigReader reader;
    readonly CBuilder cBuilder;
    readonly RunnerSettings settings;
    readonly CHeaderBuilder cHeaderBuilder;

    public CodeGenRunner(DynamicVarsResolver varsResolver, CodeGenContext codeGenContext, IRenderConfigC renderConfig, ConfigReader reader, CBuilder cBuilder, RunnerSettings settings, CHeaderBuilder cHeaderBuilder)
    {
        this.varsResolver = varsResolver;
        this.codeGenContext = codeGenContext;
        this.renderConfig = renderConfig;
        this.reader = reader;
        this.cBuilder = cBuilder;
        this.settings = settings;
        this.cHeaderBuilder = cHeaderBuilder;
    }

    public void Run()
    {
        varsResolver.Resolve();

        reader.ReadObject(renderConfig);
        cBuilder.Generate();
        cHeaderBuilder.Generate();

        string hFileContents = codeGenContext.hFileSb.ToString();
        string cFileContents = codeGenContext.cFileSb.ToString();

        File.WriteAllText($"{settings.outputDirectory}{settings.mangler.HFileName}", hFileContents);
        File.WriteAllText($"{settings.outputDirectory}{settings.mangler.CFileName}", cFileContents);
    }
}
