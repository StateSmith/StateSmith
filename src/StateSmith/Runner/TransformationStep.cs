using StateSmith.Compiling;
using System;

namespace StateSmith.Runner;

public class TransformationStep
{
    public string Id { get; init; }
    public Action<Statemachine> action;

    public TransformationStep(string id, Action<Statemachine> action)
    {
        Id = id;
        this.action = action;
    }

    public override string ToString()
    {
        return $"Id:{Id}";
    }
}
