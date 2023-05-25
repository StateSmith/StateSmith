#nullable enable
using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph;

public class RenderConfigVertex : Vertex
{
    public override void Accept(IVertexVisitor visitor)
    {
        visitor.Visit(this);
    }
}
