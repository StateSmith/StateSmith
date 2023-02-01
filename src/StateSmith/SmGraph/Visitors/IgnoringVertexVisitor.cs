using StateSmith.SmGraph;

namespace StateSmith.SmGraph.Visitors
{
    public class IgnoringVertexVisitor : VertexVisitor
    {
        public override void Visit(Vertex v)
        {
            // ignore
        }

        public override void Visit(NamedVertex v) { this.Visit((Vertex)v); }
        public override void Visit(OrthoState v) { this.Visit((NamedVertex)v); }
        public override void Visit(StateMachine v) { this.Visit((NamedVertex)v); }
        public override void Visit(State v) { this.Visit((NamedVertex)v); }

        public override void Visit(NotesVertex v) { this.Visit((Vertex)v); }

        public override void Visit(InitialState v) { this.Visit((Vertex)v); }
    }
}
