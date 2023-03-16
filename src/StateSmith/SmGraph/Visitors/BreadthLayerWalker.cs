using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph.Visitors;

public class BreadthLayerWalker
{
    public Action<IReadOnlyCollection<Vertex>> visitAction;
    private Queue<Vertex> curDepthList = new();

    private Queue<Vertex> nextDepthList = new();

    public int curDepth;

    public void Walk(Vertex root)
    {
        curDepthList.Enqueue(root);

        while (curDepthList.Any())
        {
            visitAction(curDepthList);
            curDepth++;

            foreach (var kid in curDepthList)
            {
                nextDepthList.Enqueue(kid);
            }

            // clear curDepthList and swap so that curDepthList has old nextDepthList and nextDepthList is now blank.
            var temp = curDepthList;
            curDepthList.Clear();
            curDepthList = nextDepthList;
            nextDepthList = temp;
        }
    }
}
