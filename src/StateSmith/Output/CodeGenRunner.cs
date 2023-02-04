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
    readonly CBuilder cBuilder;
    readonly RunnerSettings settings;
    readonly CHeaderBuilder cHeaderBuilder;
    readonly ConfigReader configReader;
    readonly ConfigReaderObjectProvider configReaderObjectProvider;

    public CodeGenRunner(DynamicVarsResolver varsResolver, CodeGenContext codeGenContext, ConfigReader reader, CBuilder cBuilder, RunnerSettings settings, CHeaderBuilder cHeaderBuilder, ConfigReaderObjectProvider configReaderObjectProvider)
    {
        this.varsResolver = varsResolver;
        this.codeGenContext = codeGenContext;
        this.configReader = reader;
        this.cBuilder = cBuilder;
        this.settings = settings;
        this.cHeaderBuilder = cHeaderBuilder;
        this.configReaderObjectProvider = configReaderObjectProvider;
    }

    public void Run()
    {
        varsResolver.Resolve();

        configReader.ReadObject(configReaderObjectProvider);
        cBuilder.Generate();
        cHeaderBuilder.Generate();

        string hFileContents = codeGenContext.hFileSb.ToString();
        string cFileContents = codeGenContext.cFileSb.ToString();

        File.WriteAllText($"{settings.outputDirectory}{settings.mangler.HFileName}", hFileContents);
        File.WriteAllText($"{settings.outputDirectory}{settings.mangler.CFileName}", cFileContents);
    }
}
