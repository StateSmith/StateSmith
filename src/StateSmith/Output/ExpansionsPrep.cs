#nullable enable

using StateSmith.Output.UserConfig;

namespace StateSmith.Output;

public class ExpansionsPrep
{
    protected readonly AutoExpandedVarsProcessor autoExpandedVarsProcessor;
    protected readonly DefaultExpansionsProcessor defaultExpansionsProcessor;
    protected readonly DynamicVarsResolver varsResolver;
    readonly ExpansionConfigReader expansionConfigReader;
    readonly ExpansionConfigReaderObjectProvider expansionConfigReaderObjectProvider;

    public ExpansionsPrep(AutoExpandedVarsProcessor autoExpandedVarsProcessor, DynamicVarsResolver varsResolver, ExpansionConfigReader expansionConfigReader, ExpansionConfigReaderObjectProvider expansionConfigReaderObjectProvider, DefaultExpansionsProcessor defaultExpansionsProcessor)
    {
        this.autoExpandedVarsProcessor = autoExpandedVarsProcessor;
        this.varsResolver = varsResolver;
        this.expansionConfigReader = expansionConfigReader;
        this.expansionConfigReaderObjectProvider = expansionConfigReaderObjectProvider;
        this.defaultExpansionsProcessor = defaultExpansionsProcessor;
    }

    public void PrepForCodeGen()
    {
        varsResolver.Resolve();
        expansionConfigReader.ReadObject(expansionConfigReaderObjectProvider);
        autoExpandedVarsProcessor.AddExpansions();
        defaultExpansionsProcessor.AddExpansions();
    }
}
