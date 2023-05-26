using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class ExitPoint : PseudoStateVertex
    {
        public string label;

        public ExitPoint(string label)
        {
            this.label = label;
        }

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
