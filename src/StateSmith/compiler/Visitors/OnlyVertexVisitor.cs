using StateSmith.Compiling;

namespace StateSmith.compiler.Visitors
{
    public abstract class OnlyVertexVisitor : VertexVisitor
    {
        public override void Visit(NamedVertex v)
        {
            Visit((Vertex)v);
        }

        public override void Visit(State v)
        {
            Visit((Vertex)v);
        }

        public override void Visit(OrthoState v)
        {
            Visit((Vertex)v);
        }
        public override void Visit(StateMachine v)
        {
            Visit((Vertex)v);
        }
        public override void Visit(NotesVertex v)
        {
            Visit((Vertex)v);
        }
        public override void Visit(InitialState v)
        {
            Visit((Vertex)v);
        }
    }
}
