namespace StateSmith.SmGraph.Visitors;

/// <summary>
/// Similar to Antlr4 walker. Automatically visits children. You just have
/// to listen to enter and exit callbacks.
/// </summary>
public class VertexWalker
{
    LambdaVertexVisitor visitor = new LambdaVertexVisitor();

    public void Walk(Vertex graph)
    {
        visitor.visitAction = v =>
        {
            VertexEntered(v);
            VertexVisitor.VisitVertexChildren(v, visitor);
            VertexExited(v);
        };
        graph.Accept(visitor);
    }

    public virtual void VertexEntered(Vertex v) { }
    public virtual void VertexExited(Vertex v) { }
}
