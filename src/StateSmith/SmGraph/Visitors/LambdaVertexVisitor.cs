using StateSmith.SmGraph;
using System;

namespace StateSmith.SmGraph.Visitors
{
    public class LambdaVertexVisitor : VertexVisitor
    {
        public Action<Vertex> visitAction;

        public override void Visit(Vertex v)
        {
            visitAction(v);
        }
    }
}
