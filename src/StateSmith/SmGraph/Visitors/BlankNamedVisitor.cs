using StateSmith.SmGraph;

namespace StateSmith.SmGraph.Visitors
{
    /// <summary>
    /// Extend this class if you only want to have to implement `Visit(NamedVertex v)`
    /// </summary>
    public abstract class BlankNamedVisitor : NamedVisitor
    {
        public override void Visit(Vertex v)
        {
            // ignore
        }

        public override void Visit(NotesVertex v)
        {
            // ignore
        }

        public override void Visit(InitialState v)
        {
            // ignore
        }
    }
}
