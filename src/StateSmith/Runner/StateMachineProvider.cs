#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph;

namespace StateSmith.Runner;

public class StateMachineProvider : IStateMachineProvider
{
    StateMachine? _machine;

    public StateMachineProvider()
    {

    }

    public StateMachineProvider(StateMachine machine)
    {
        _machine = machine;
    }

    public void SetStateMachine(StateMachine machine)
    {
        _machine = machine;
    }

    public StateMachine GetStateMachine()
    {
        return _machine.ThrowIfNull();
    }
}
