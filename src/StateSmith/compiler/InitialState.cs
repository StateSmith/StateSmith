using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class InitialState : Vertex
    {
        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
