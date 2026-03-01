#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class BerryExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "self.vars.";
}
