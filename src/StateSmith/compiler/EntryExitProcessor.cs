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

            var transitionsToReTarget = GetParentEntryTransitions(entryPoint, parentState);

            foreach (var parentIncomingTransition in transitionsToReTarget.ToList())
            {
                parentIncomingTransition.RetargetTo(entryPoint);
                parentIncomingTransition.viaEntry = null;
            }
        }

        private static IEnumerable<Behavior> GetParentEntryTransitions(EntryPoint entryPoint, NamedVertex parentState)
        {
            var matching = parentState.IncomingTransitions.Where(b => b.viaEntry == entryPoint.label);

            if (!matching.Any())
            {
                throw new VertexValidationException(entryPoint, $"No transitions match entry point with `via entry {entryPoint.label}`.");
            }

            return matching;
        }

        public override void Visit(ExitPoint exitPoint)
        {
            NamedVertex parentState = (NamedVertex)exitPoint.Parent;    // todolow - provide nicer exception or validate somewhere else

            var transitionsToReTarget = GetParentExitTransitionsAndValidate(exitPoint, parentState);

            foreach (var transitionToReTarget in transitionsToReTarget.ToList())
            {
                transitionToReTarget.viaExit = null;
                transitionToReTarget.RetargetOwner(exitPoint);
            }
        }

        private static IEnumerable<Behavior> GetParentExitTransitionsAndValidate(ExitPoint exitPoint, NamedVertex parentState)
        {
            var matching = parentState.Behaviors.Where(b => b.HasTransition() && b.viaExit == exitPoint.label);

            if (!matching.Any())
            {
                throw new VertexValidationException(exitPoint, $"No transitions match exit point with `via exit {exitPoint.label}`.");
            }

            return matching;
        }

        public override void Visit(NotesVertex v)
        {
            // do nothing. don't visit children
        }

    }
}
