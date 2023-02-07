using StateSmith.SmGraph;

#nullable enable

namespace StateSmith.Runner;

public interface IStateMachineProvider
{
    StateMachine GetStateMachine();
}
