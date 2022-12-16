using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class ChoicePoint : PseudoStateVertex
    {
        public string label;

        public ChoicePoint(string label = "")
        {
            this.label = label;
        }

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
