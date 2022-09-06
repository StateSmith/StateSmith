using StateSmith.Compiling;

namespace StateSmith.compiler.Visitors
{
    public class DummyVertexVisitor : VertexVisitor
    {
        public override void Visit(Vertex v) { VisitChildren(v); }
        public override void Visit(NamedVertex v) { VisitChildren(v); }
        public override void Visit(OrthoState v) { VisitChildren(v); }
        public override void Visit(Statemachine v) { VisitChildren(v); }
        public override void Visit(NotesVertex v) { VisitChildren(v); }
        public override void Visit(InitialState v) { VisitChildren(v); }
        public override void Visit(State v) { VisitChildren(v); }
    }
}
