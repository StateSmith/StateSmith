using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class InitialState : PseudoStateVertex
    {
        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
