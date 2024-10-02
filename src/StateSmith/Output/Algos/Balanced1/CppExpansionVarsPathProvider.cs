#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class CppExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "this->vars.";
}
