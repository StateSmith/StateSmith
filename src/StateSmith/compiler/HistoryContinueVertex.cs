#nullable enable

using StateSmith.compiler.Visitors;
using System.Collections.Generic;

namespace StateSmith.Compiling
{
    public class HistoryContinueVertex : PseudoStateVertex
    {
        internal List<HistoryVertex> historyVertices = new();

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
