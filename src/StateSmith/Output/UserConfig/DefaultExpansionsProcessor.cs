#nullable enable

using StateSmith.Input.Expansions;
using System;

namespace StateSmith.Output.UserConfig;

public class DefaultExpansionsProcessor
{
    private readonly RenderConfigBaseVars renderConfig;
    private readonly IExpander expander;
    private readonly IExpansionVarsPathProvider expansionVarsPathProvider;

    public DefaultExpansionsProcessor(RenderConfigBaseVars renderConfig, IExpander expander, IExpansionVarsPathProvider expansionVarsPathProvider)
    {
        this.renderConfig = renderConfig;
        this.expander = expander;
        this.expansionVarsPathProvider = expansionVarsPathProvider;
    }

    public void AddExpansions()
    {
        string? anyTemplate = GetNonEmptyOrDefault(renderConfig.DefaultAnyExpTemplate, null);
        MaybeAddExpander(renderConfig.DefaultVarExpTemplate, anyTemplate, expander.SetDefaultVarExpander);
        MaybeAddExpander(renderConfig.DefaultFuncExpTemplate, anyTemplate, expander.SetDefaultFuncExpander);
    }

    public void MaybeAddExpander(string? template, string? defaultTemplate, Action<DefaultExpander> addingFunction)
    {
        var expander = MaybeBuildExpander(template, defaultTemplate);
        if (expander != null)
        {
            addingFunction(expander);
        }
    }

    private DefaultExpander? MaybeBuildExpander(string? template, string? defaultTemplate)
    {
        template = GetNonEmptyOrDefault(template, defaultTemplate);

        if (template == null)
        {
            return null;
        }

        return new DefaultExpander(expansionVarsPathProvider, template);
    }

    private static string? GetNonEmptyOrDefault(string? value, string? defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
