using StateSmith.SmGraph.Validation;
using StateSmith.SmGraph.Visitors;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/59
/// </summary>
public class OrderAndElseProcessor : VertexVisitor
{
    public static void Process(Vertex v)
    {
        var processor = new OrderAndElseProcessor();
        v.Accept(processor);
    }

    public override void Visit(Vertex v)
    {
        int elseCount = 0;

        foreach (var b in v.Behaviors)
        {
            elseCount = ValidateBehavior(elseCount, b);
            SortBehaviorsByOrder(v);
        }

        VisitChildren(v);
    }

    public static void SortBehaviorsByOrder(Vertex v)
    {
        v._behaviors = v.Behaviors.OrderBy((b) => b.order).ToList();
    }

    private static int ValidateBehavior(int elseCount, Behavior b)
    {
        if (b.Triggers.Contains("else"))
        {
            elseCount++;

            if (elseCount > 1)
            {
                throw new BehaviorValidationException(b, "a state may only mark a single transition with `else`");
            }

            if (b.HasTransition() == false)
            {
                throw new BehaviorValidationException(b, "`else` is only valid when specified on a transition (for now)");
            }

            if (b.order != Behavior.DEFAULT_ORDER && b.order != Behavior.ELSE_ORDER)
            {
                throw new BehaviorValidationException(b, "you cannot specify an order with `else`");
            }

            if (b.HasGuardCode())
            {
                throw new BehaviorValidationException(b, "you cannot specify a guard with an `else` \"trigger\"");
            }

            b.order = Behavior.ELSE_ORDER;
            b._triggers.Remove("else");
            if (b.HasAtLeastOneTrigger()) //shouldn't have any left after removal of "else"
            {
                throw new BehaviorValidationException(b, "you cannot specify triggers with an `else` \"trigger\"");
            }
        }

        return elseCount;
    }
}
