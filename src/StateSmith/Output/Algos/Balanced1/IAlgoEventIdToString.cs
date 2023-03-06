#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public interface IAlgoEventIdToString
{
    void CreateEventIdToStringFunction(OutputFile file, StateMachine sm);
}
