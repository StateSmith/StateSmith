#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class SwiftExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "self.vars.";
}
