using StateSmith.compiler.Visitors;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StateSmith.Compiling
{
    public class EntryExitProcessor : DummyVertexVisitor
    {

        public override void Visit(EntryPoint entryPoint)
        {
            NamedVertex parentState = (NamedVertex)entryPoint.Parent;    // todolow - provide nicer exception or validate somewhere else

            var entryPointTransition = entryPoint.Behaviors.Single();

            var transitionsToReTarget = GetParentEntryTransitions(entryPoint, parentState);

            foreach (var transition in transitionsToReTarget.ToList())
            {
                // retarget to entryPoint target and copy behavior
                transition.actionCode = transition.actionCode ?? "";
                transition.actionCode += entryPointTransition.actionCode ?? "";
                transition.RetargetTo(entryPointTransition.TransitionTarget);
                transition.viaEntry = null;
            }

            // remove entry point behavior prior to removing
            entryPoint._behaviors.Clear();
            parentState.RemoveChild(entryPoint);
        }

        private IEnumerable<Behavior> GetParentEntryTransitions(EntryPoint entryPoint, NamedVertex parentState)
        {
            var matching = parentState.IncomingTransitions.Where(b => b.viaEntry == entryPoint.label);

            switch (matching.Count())
            {
                case 0: throw new VertexValidationException(entryPoint, $"No transitions match entry point with `via entry {entryPoint.label}`.");
                default: break;
            }

            return matching;
        }

        public override void Visit(ExitPoint exitPoint)
        {
            NamedVertex parentState = (NamedVertex)exitPoint.Parent;    // todolow - provide nicer exception or validate somewhere else

            Behavior parentExitTransition = GetParentExitTransitionAndValidate(exitPoint, parentState);

            foreach (var transitionToReTarget in exitPoint.IncomingTransitions.ToList())
            {
                // copy parent transition action code to incoming transition
                transitionToReTarget.actionCode = transitionToReTarget.actionCode ?? "";
                transitionToReTarget.actionCode += parentExitTransition.actionCode ?? "";
                transitionToReTarget.viaEntry = parentExitTransition.viaEntry;
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
                case 0: throw new VertexValidationException(exitPoint, $"No transitions match exit point with `via exit {exitPoint.label}`.");
                case 1: break;
                default: throw new VertexValidationException(exitPoint, $"Too many transitions match exit point with `via exit {exitPoint.label}`.");
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
