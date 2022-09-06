using StateSmith.Compiling;
using System;

namespace StateSmith.compiler.Visitors
{
    public class LambdaVertexVisitor : OnlyVertexVisitor
    {
        public Action<Vertex> visitAction;

        public override void Visit(Vertex v)
        {
            visitAction(v);
        }
    }
}
