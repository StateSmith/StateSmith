using StateSmith.SmGraph.Validation;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/136
/// </summary>
public class ElseGuardProcessor
{
    public static void Process(StateMachine sm)
    {
        sm.VisitTypeRecursively<Vertex>(vertex =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                if (behavior.guardCode.Trim().ToLower() == "else")
                {
                    if (behavior.order != Behavior.DEFAULT_ORDER)
                        throw new BehaviorValidationException(behavior, "can't specify order and `[else]` guard at the same time.");

                    behavior.guardCode = "";
                    behavior.order = Behavior.ELSE_ORDER;
                }
            }
        });
    }
}
