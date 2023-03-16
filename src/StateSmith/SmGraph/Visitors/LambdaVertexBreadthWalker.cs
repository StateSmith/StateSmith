using System;
using System.Collections.Generic;

namespace StateSmith.SmGraph.Visitors;

public class LambdaVertexBreadthWalker
{
    public Action<Vertex> visitAction;
    private Queue<Vertex> toVisit = new();

    public void Walk(Vertex root)
    {
        toVisit.Enqueue(root);

        while (toVisit.Count > 0)
        {
            var v = toVisit.Dequeue();
            visitAction(v);

            foreach (var kid in v.Children)
            {
                toVisit.Enqueue(kid);
            }
        }
    }
}
