using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Input.Expansions;

/// <summary>
/// This class does expansions and tracks attempts for expansions.
/// This can be useful if you want to provide an auto mocking feature.
/// </summary>
public class TrackingExpander : Expander
{
    protected readonly HashSet<string> attemptedVariableExpansions = new();
    protected readonly HashSet<string> attemptedFunctionExpansions = new();

    public virtual ISet<string> AttemptedVariableExpansions => attemptedVariableExpansions;
    public virtual ISet<string> AttemptedFunctionExpansions => attemptedFunctionExpansions;

    public virtual ISet<string> FailedVariableExpansions => new HashSet<string>(AttemptedVariableExpansions.Except(GetVariableNames()));
    public virtual ISet<string> FailedFunctionExpansions => new HashSet<string>(AttemptedFunctionExpansions.Except(GetFunctionNames()));

    public override string TryExpandVariableExpansion(string name)
    {
        attemptedVariableExpansions.Add(name);
        return base.TryExpandVariableExpansion(name);
    }

    public override string TryExpandFunctionExpansion(string name, string[] args, string rawBracedFuncArgs = "")
    {
        attemptedFunctionExpansions.Add(name);
        return base.TryExpandFunctionExpansion(name, args, rawBracedFuncArgs);
    }

    public override bool HasFunctionName(string name)
    {
        attemptedFunctionExpansions.Add(name);
        return base.HasFunctionName(name);
    }
}
