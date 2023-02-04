#nullable enable
using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph;

public class RenderConfigVertex : Vertex
{
    public override void Accept(VertexVisitor visitor)
    {
        visitor.Visit(this);
    }
}
