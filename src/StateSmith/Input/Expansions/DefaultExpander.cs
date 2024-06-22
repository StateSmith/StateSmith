#nullable enable

using StateSmith.Output;

namespace StateSmith.Input.Expansions;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/284
/// </summary>
public class DefaultExpander
{
    private readonly IExpansionVarsPathProvider expansionVarsPathProvider;
    private string template = string.Empty;

    public DefaultExpander(IExpansionVarsPathProvider expansionVarsPathProvider, string template)
    {
        this.expansionVarsPathProvider = expansionVarsPathProvider;
        SetTemplate(template);
    }

    public void SetTemplate(string template)
    {
        var varsPath = expansionVarsPathProvider.ExpansionVarsPath;
        template = template.Replace("{VarsPath}", varsPath);

        this.template = template;
    }

    public string Process(string memberName)
    {
        var result = template;
        result = result.Replace("{AutoNameCopy()}", memberName);
        result = result.Replace("{AutoVarName()}", expansionVarsPathProvider.ExpansionVarsPath + memberName);

        return result;
    }
}
