using StateSmith.compiler.Visitors;

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
        }

        public override void Visit(NotesVertex v)
        {
            // ignore
        }
    }
}
