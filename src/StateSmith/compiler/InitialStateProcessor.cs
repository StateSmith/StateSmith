using StateSmith.compiler.Visitors;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Compiling
{
    /// <summary>
    /// Assumes validation already done
    /// </summary>
    public class InitialStateProcessor : DummyVertexVisitor
    {
        public override void Visit(InitialState initialState)
        {
            var parent = initialState.Parent;

            var initialStateTransition = initialState.Behaviors[0];

            // don't process simplification if this is the root initial state
            //TODO low. Create method to detect if parent is root/state machine.
            if (parent.Parent == null)
            {
                return;
            }

            var newTarget = initialStateTransition.TransitionTarget;

            var parentIncomingTransitions = parent.IncomingTransitions.ToList();
            foreach (var incomingTransition in parentIncomingTransitions)
            {
                //transitions to parent will be moved to transitions to initial state target

                //validate behavior variables. TODO low cleanup. Seems confusing.
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
}
