using StateSmith.compiler.Visitors;
using System.Linq;

namespace StateSmith.Compiling
{
    public class VertexValidator : OnlyVertexVisitor
    {
        public override void Visit(Vertex v)
        {
            foreach (var b in v.Behaviors)
            {
                ValidateBehavior(v, b);
            }

            VisitChildren(v);
        }

        private static void ValidateBehavior(Vertex v, Behavior b)
        {
            if (b.HasTransition() == false)
            {
                if (b.viaEntry != null || b.viaExit != null)
                {
                    throw new VertexValidationException(v, "via entry/exit can only be specified on transitions");
                }
            }
            else
            {
                ValidateViaEntry(b);

                ValidateViaExit(v, b);
            }

        }

        private static void ValidateViaExit(Vertex v, Behavior b)
        {
            string exitLabel = b.viaExit;
            if (exitLabel == null)
            {
                return;
            }

            // find exit point with matching label
            var matchingExitPoints = v.Children.OfType<ExitPoint>().Where(point => point.label == exitLabel);

            switch (matchingExitPoints.Count())
            {
                case 0: throw new BehaviorValidationException(b, $"no matching exit point found with label `{exitLabel}`.");
                case 1: break; // happy path
                default: throw new BehaviorValidationException(b, $"multiple matching exit points found with label `{exitLabel}`.");
            }
        }

        private static void ValidateViaEntry(Behavior b)
        {
            string entryLabel = b.viaEntry;
            if (entryLabel == null)
            {
                return;
            }

            var matchingEntryPoints = b.TransitionTarget.Children.OfType<EntryPoint>().Where(point => point.label == entryLabel);

            switch (matchingEntryPoints.Count())
            {
                case 0: throw new BehaviorValidationException(b, $"no matching entry point found with label `{entryLabel}`.");
                case 1: break; // happy path
                default: throw new BehaviorValidationException(b, $"multiple matching entry points found with label `{entryLabel}`.");
            }
        }

        public override void Visit(NotesVertex v)
        {
            // ignore
        }
    }
}
