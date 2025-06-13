#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public interface IAlgoEventIdFuncGenerator
{
    void CreateEventIdToStringFunction(OutputFile file, StateMachine sm);
    void CreateEventIdIsValidFunction(OutputFile file, StateMachine sm);
}
