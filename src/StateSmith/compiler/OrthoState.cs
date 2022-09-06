using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class OrthoState : State
    {
        public double order;

        public OrthoState(string name) : base(name)
        {
        }

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
