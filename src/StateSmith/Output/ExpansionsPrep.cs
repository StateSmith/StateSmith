using StateSmith.Output.UserConfig;

#nullable enable

namespace StateSmith.Output;

public class ExpansionsPrep
{
    protected readonly AutoExpandedVarsProcessor autoExpandedVarsProcessor;
    protected readonly DynamicVarsResolver varsResolver;
    readonly ExpansionConfigReader expansionConfigReader;
    readonly ExpansionConfigReaderObjectProvider expansionConfigReaderObjectProvider;

    public ExpansionsPrep(AutoExpandedVarsProcessor autoExpandedVarsProcessor, DynamicVarsResolver varsResolver, ExpansionConfigReader expansionConfigReader, ExpansionConfigReaderObjectProvider expansionConfigReaderObjectProvider)
    {
        this.autoExpandedVarsProcessor = autoExpandedVarsProcessor;
        this.varsResolver = varsResolver;
        this.expansionConfigReader = expansionConfigReader;
        this.expansionConfigReaderObjectProvider = expansionConfigReaderObjectProvider;
    }

    public void PrepForCodeGen()
    {
        varsResolver.Resolve();
        expansionConfigReader.ReadObject(expansionConfigReaderObjectProvider);
        autoExpandedVarsProcessor.AddExpansions();
    }
}
