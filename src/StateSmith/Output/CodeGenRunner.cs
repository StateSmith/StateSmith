using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System.IO;

#nullable enable

namespace StateSmith.Output;

public class CodeGenRunner : ICodeGenRunner
{
    readonly DynamicVarsResolver varsResolver;
    readonly CodeGenContext codeGenContext;
    readonly CHeaderBuilder cHeaderBuilder;
    readonly CBuilder cBuilder;
    readonly RunnerSettings settings;
    readonly ExpansionConfigReader expansionConfigReader;
    readonly ExpansionConfigReaderObjectProvider expansionConfigReaderObjectProvider;
    readonly AutoExpandedVarsProcessor autoExpandedVarsProcessor;
    readonly ICodeFileWriter codeFileWriter;

    public CodeGenRunner(DynamicVarsResolver varsResolver, CodeGenContext codeGenContext, ExpansionConfigReader configReader, CBuilder cBuilder, RunnerSettings settings, CHeaderBuilder cHeaderBuilder, ExpansionConfigReaderObjectProvider configReaderObjectProvider, AutoExpandedVarsProcessor autoExpandedVarsProcessor, ICodeFileWriter codeFileWriter)
    {
        this.varsResolver = varsResolver;
        this.codeGenContext = codeGenContext;
        this.expansionConfigReader = configReader;
        this.cBuilder = cBuilder;
        this.settings = settings;
        this.cHeaderBuilder = cHeaderBuilder;
        this.expansionConfigReaderObjectProvider = configReaderObjectProvider;
        this.autoExpandedVarsProcessor = autoExpandedVarsProcessor;
        this.codeFileWriter = codeFileWriter;
    }

    public void Run()
    {
        varsResolver.Resolve();
        expansionConfigReader.ReadObject(expansionConfigReaderObjectProvider);
        autoExpandedVarsProcessor.AddExpansions();

        cBuilder.Generate();
        cHeaderBuilder.Generate();

        string hFileContents = codeGenContext.hFileSb.ToString();
        string cFileContents = codeGenContext.cFileSb.ToString();

        codeFileWriter.WriteFile(filePath: $"{settings.outputDirectory}{settings.mangler.HFileName}", hFileContents);
        codeFileWriter.WriteFile(filePath: $"{settings.outputDirectory}{settings.mangler.CFileName}", cFileContents);
    }
}
