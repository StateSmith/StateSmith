using StateSmith.Input.Expansions;
using StateSmith.SmGraph;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Output.UserConfig;

public class UserExpansionScriptBases
{
    protected readonly List<UserExpansionScriptBase> scriptBases = new();

    public void Add(UserExpansionScriptBase script)
    {
        scriptBases.Add(script);
    }

    public void UpdateCurrentBehavior(Behavior behavior)
    {
        foreach (var scriptBase in scriptBases)
        {
            scriptBase.CurrentBehavior = behavior;
        }
    }
}
