#nullable enable

using StateSmith.SmGraph.Visitors;
using System.Collections.Generic;

namespace StateSmith.SmGraph
{
    public class HistoryContinueVertex : PseudoStateVertex
    {
        internal List<HistoryVertex> historyVertices = new();

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
