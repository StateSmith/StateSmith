#nullable enable

using StateSmith.Input.Expansions;

namespace StateSmith.Output.UserConfig;

public class AutoExpandedVarsProcessor
{
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IExpander expander;
    private readonly IExpansionVarsPathProvider expansionVarsPathProvider;
    private readonly IAutoVarsParser autoVarsParser;

    public AutoExpandedVarsProcessor(RenderConfigBaseVars renderConfig, IExpander expander, IExpansionVarsPathProvider expansionVarsPathProvider, IAutoVarsParser autoVarsParser)
    {
        this.renderConfig = renderConfig;
        this.expander = expander;
        this.expansionVarsPathProvider = expansionVarsPathProvider;
        this.autoVarsParser = autoVarsParser;
    }

    public void AddExpansions()
    {
        string toParse = renderConfig.AutoExpandedVars;
        var varsPath = expansionVarsPathProvider.ExpansionVarsPath;

        var identifiers = autoVarsParser.ParseIdentifiers(toParse);

        foreach (var identifier in identifiers)
        {
            expander.AddVariableExpansion(identifier, varsPath + identifier);
        }

        StringUtils.AppendInPlaceWithNewlineIfNeeded(ref renderConfig.VariableDeclarations, renderConfig.AutoExpandedVars);
    }
}
