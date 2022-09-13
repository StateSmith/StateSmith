using StateSmith.compiler.Visitors;
using System.Linq;

namespace StateSmith.Compiling
{
    public class EntryExitProcessor : DummyVertexVisitor
    {
        // handle exit points first as they are generally more useful

        // find an exit point
        // find parent transition that matches exit point label

        // foreach incoming transition
        // copy parent transition action code to incoming transition
        // re-target incoming transition to new destination

        // remove exit point

        public override void Visit(ExitPoint exitPoint)
        {
            NamedVertex parentState = (NamedVertex)exitPoint.Parent;    // todolow - provide nicer exception or validate somewhere else

            Behavior parentExitTransition = GetParentExitTransitionAndValidate(exitPoint, parentState);

            foreach (var transitionToReTarget in exitPoint.IncomingTransitions.ToList())
            {
                // copy parent transition action code to incoming transition
                transitionToReTarget.actionCode = transitionToReTarget.actionCode ?? "";
                transitionToReTarget.actionCode += parentExitTransition.actionCode ?? "";
                transitionToReTarget.RetargetTo(parentExitTransition.TransitionTarget);
            }

            parentState._behaviors.Remove(parentExitTransition);
            parentState.RemoveChild(exitPoint);
        }

        private static Behavior GetParentExitTransitionAndValidate(ExitPoint exitPoint, NamedVertex parentState)
        {
            var matching = parentState.Behaviors.Where(b => b.HasTransition() && b.viaExit == exitPoint.label);

            switch (matching.Count())
            {
                case 0:
                    throw new VertexValidationException(exitPoint, $"No transitions match exit point with `via exit {exitPoint.label}`.");
                case 1:
                    break;
                default:
                    throw new VertexValidationException(exitPoint, $"Too many transitions match exit point with `via exit {exitPoint.label}`.");
            }

            var parentExitTransition = matching.First();

            if (parentState.ContainsVertex(parentExitTransition.TransitionTarget))
            {
                throw new BehaviorValidationException(parentExitTransition, "An exit transition must target something out of the state being exited.");
            }

            return parentExitTransition;
        }

        public override void Visit(NotesVertex v)
        {
            // do nothing. don't visit children
        }

    }
}
