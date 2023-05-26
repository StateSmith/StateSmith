using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class OrthoState : State
    {
        public double order;

        public OrthoState(string name) : base(name)
        {
        }

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
