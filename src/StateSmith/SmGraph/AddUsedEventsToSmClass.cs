using StateSmith.Common;

#nullable enable

namespace StateSmith.SmGraph;

public class AddUsedEventsToSmClass
{
    public static void Process(StateMachine stateMachine)
    {
        stateMachine.VisitRecursively(v =>
        {
            foreach (var behavior in v.Behaviors)
            {
                foreach (var trigger in behavior.Triggers)
                {
                    TriggerHelper.MaybeAddEventToSm(stateMachine, behavior, trigger);
                }
            }
        });

        // https://github.com/StateSmith/StateSmith/issues/121
        if (stateMachine._events.Count == 0)
        {
            stateMachine._events.Add(TriggerHelper.TRIGGER_DO);
        }
    }
}
