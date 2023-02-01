using StateSmith.Common;
using StateSmith.compiler.Visitors;

#nullable enable

namespace StateSmith.Compiling;

public class AddUsedEventsToSmClass : VertexWalker
{
    private readonly StateMachine stateMachine;

    public AddUsedEventsToSmClass(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public static void Process(StateMachine stateMachine)
    {
        var cls = new AddUsedEventsToSmClass(stateMachine);
        cls.Walk(stateMachine);
    }

    private void ProcessBehaviors(Vertex v)
    {
        foreach (var behavior in v.Behaviors)
        {
            foreach (var trigger in behavior.triggers)
            {
                TriggerHelper.MaybeAddEvent(stateMachine, behavior, trigger);
            }
        }
    }

    public override void VertexEntered(Vertex v)
    {
        ProcessBehaviors(v);
    }
}
