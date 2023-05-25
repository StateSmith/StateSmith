#nullable enable

using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph;

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
