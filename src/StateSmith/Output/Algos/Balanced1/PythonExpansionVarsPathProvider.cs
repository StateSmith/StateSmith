#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class PythonExpansionVarsPathProvider : IExpansionVarsPathProvider
{
    public string ExpansionVarsPath => "self.vars.";
}
