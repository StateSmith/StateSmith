#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output.Algos.Balanced1;

public interface IAlgoStateIdToString
{
    void CreateStateIdToStringFunction(OutputFile file, StateMachine sm);
}
