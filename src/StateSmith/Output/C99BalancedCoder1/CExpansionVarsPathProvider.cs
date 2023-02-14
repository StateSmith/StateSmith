#nullable enable

namespace StateSmith.Output.C99BalancedCoder1;

public class CExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "self->vars.";
}
