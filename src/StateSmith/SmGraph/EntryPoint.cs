using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class EntryPoint : PseudoStateVertex
    {
        public string label;

        public EntryPoint(string label)
        {
            this.label = label;
        }

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
