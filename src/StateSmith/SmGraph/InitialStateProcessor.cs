using StateSmith.SmGraph.Validation;
using StateSmith.SmGraph.Visitors;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// Currently unused! See https://github.com/StateSmith/StateSmith/issues/67
/// Assumes validation already done.
/// </summary>
public class InitialStateProcessor : RecursiveVertexVisitor
{
    public static void Process(Vertex v)
    {
        var processor = new InitialStateProcessor();
        v.Accept(processor);
    }

    public override void Visit(InitialState initialState)
    {
        var parent = initialState.Parent;

        var initialStateTransition = initialState.Behaviors[0];

        // don't process simplification if this is the root initial state
        // todo_low. Create method to detect if parent is root/state machine.
        if (parent.Parent == null)
        {
            return;
        }

        var newTarget = initialStateTransition.TransitionTarget;

        var parentIncomingTransitions = parent.IncomingTransitions.ToList();
        foreach (var incomingTransition in parentIncomingTransitions)
        {
            // transitions to parent will be moved to transitions to initial state target only if they don't have an entry point specified
            if (incomingTransition.viaEntry?.Length > 0)
            {
                continue;
            }

            // validate behavior variables. todo_low cleanup. Seems confusing.
            if (incomingTransition.TransitionTarget != parent)
            {
                throw new BehaviorValidationException(incomingTransition, "Inconsistent behavior 549846");
            }
            incomingTransition.actionCode += initialStateTransition.actionCode;

            incomingTransition.RetargetTo(newTarget);
        }

        parent.RemoveChild(initialState);
    }
}
