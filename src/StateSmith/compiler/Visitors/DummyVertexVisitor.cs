using StateSmith.Compiling;

namespace StateSmith.compiler.Visitors
{
    public class DummyVertexVisitor : VertexVisitor
    {
        public override void Visit(Vertex v) { VisitChildren(v); }

        public override void Visit(NamedVertex v) { VisitChildren(v); }
        public override void Visit(OrthoState v) { this.Visit((NamedVertex)v); }
        public override void Visit(StateMachine v) { this.Visit((NamedVertex)v); }
        public override void Visit(State v) { this.Visit((NamedVertex)v); }

        public override void Visit(NotesVertex v) { VisitChildren(v); }
        public override void Visit(InitialState v) { VisitChildren(v); }
    }
}
