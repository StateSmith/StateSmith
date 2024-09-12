#nullable enable

using StateSmith.SmGraph;

namespace StateSmith.Output;

public interface IGilAlgo
{
    string GenerateGil(StateMachine sm);
}
