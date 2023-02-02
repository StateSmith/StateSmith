#nullable enable

namespace StateSmith.Output.C99BalancedCoder1;

internal class CExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "self->vars.";
}
