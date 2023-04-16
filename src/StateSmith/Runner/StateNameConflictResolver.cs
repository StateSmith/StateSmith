#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Runner;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/138
/// </summary>
public class StateNameConflictResolver
{
    readonly RunnerSettings runnerSettings;

    public StateNameConflictResolver(RunnerSettings runnerSettings)
    {
        this.runnerSettings = runnerSettings;
    }

    public virtual void ResolveNameConflicts(StateMachine sm)
    {
        RunnerSettings.NameConflictResolution resolveSetting = runnerSettings.nameConflictResolution;

        bool useShortFqnAncestor = resolveSetting == RunnerSettings.NameConflictResolution.ShortFqnAncestor;
        bool useShortFqnParent = resolveSetting == RunnerSettings.NameConflictResolution.ShortFqnParent;

        if (useShortFqnAncestor || useShortFqnParent)
        {
            ShortFqnNamer namer = new(resolveWithAncestor: useShortFqnAncestor);
            namer.ResolveNameConflicts(sm);
        }
    }
}
