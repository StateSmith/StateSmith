#nullable enable

namespace StateSmith.SmGraph.Visitors;

public class RecursiveVertexVisitor : VertexVisitor
{
    public override void Visit(Vertex v) { VisitChildren(v); }
}
