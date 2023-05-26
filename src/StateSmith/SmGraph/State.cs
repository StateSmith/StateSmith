using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class State : NamedVertex
    {
        public State(string name) : base(name)
        {
        }

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
