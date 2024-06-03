using StateSmith.SmGraph;
using System;

namespace StateSmith.Runner;

public class TransformationStep
{
    public string Id { get; init; }
    public Action<StateMachine> action;

    /// <summary>
    /// This constructor is preferred if you don't care what ID the transformation step has.
    /// Most people will use this constructor unless you are providing a library/plugin for others to use.
    /// <see cref="TransformationStep(string, Action{StateMachine})"/> if you want to provide an ID.
    /// </summary>
    /// <param name="action"></param>
    public TransformationStep(Action<StateMachine> action) : this("unspecified-id", action)
    {

    }

    /// <summary>
    /// The ID is only useful if you want to allow another transformation step to be able to use it.
    /// This might be helpful if you are providing a library/plugin for others to use.
    /// Otherwise, you can just use <see cref="TransformationStep(Action{StateMachine})"/>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="action"></param>
    public TransformationStep(Enum id, Action<StateMachine> action) : this(id.ToString(), action)
    {
    }

    /// <summary>
    /// The ID is only useful if you want to allow another transformation step to be able to use it.
    /// This might be helpful if you are providing a library/plugin for others to use.
    /// Otherwise, you can just use <see cref="TransformationStep(Action{StateMachine})"/>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="action"></param>
    public TransformationStep(string id, Action<StateMachine> action)
    {
        Id = id;
        this.action = action;
    }

    public static implicit operator TransformationStep(Action<StateMachine> action)
    {
        return new TransformationStep(action);
    }

    public override string ToString()
    {
        return $"Id:{Id}";
    }
}
