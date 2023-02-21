using StateSmith.SmGraph;
#nullable enable

namespace StateSmith.Output;

public interface IGilAlgo
{
    string GenerateGil(StateMachine sm);
}
