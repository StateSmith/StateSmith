#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph;

namespace StateSmith.Runner;

// TODO convert to use Func<StateMachine> instead of StateMachineProvider
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
