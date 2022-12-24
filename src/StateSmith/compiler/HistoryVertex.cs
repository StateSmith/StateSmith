#nullable enable

using StateSmith.compiler.Visitors;
using System.Collections.Generic;

namespace StateSmith.Compiling
{
    public class ShallowHistoryVertex : PseudoStateVertex
    {
        internal string stateTrackingVarName = "";
        internal int trackedStateCount = 0;

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class HistoryContinueVertex : PseudoStateVertex
    {
        internal List<ShallowHistoryVertex> historyVertices = new();

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
