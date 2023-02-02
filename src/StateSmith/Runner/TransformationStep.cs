using StateSmith.SmGraph;
using System;

namespace StateSmith.Runner;

public class TransformationStep
{
    public string Id { get; init; }
    public Action<StateMachine> action;

    public TransformationStep(string id, Action<StateMachine> action)
    {
        Id = id;
        this.action = action;
    }

    public override string ToString()
    {
        return $"Id:{Id}";
    }
}
