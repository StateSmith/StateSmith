#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class CSharpExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "this.vars.";
}
