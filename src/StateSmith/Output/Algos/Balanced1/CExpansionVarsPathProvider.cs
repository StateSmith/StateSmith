#nullable enable

using StateSmith;

namespace StateSmith.Output.Algos.Balanced1;

public class CExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "sm->vars.";
}
