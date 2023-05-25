#nullable enable
using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph;

public class InitialState : PseudoStateVertex
{
    public override void Accept(IVertexVisitor visitor)
    {
        visitor.Visit(this);
    }
}
