#nullable enable

using System.Collections.Generic;
using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class HistoryVertex : PseudoStateVertex
    {
        internal string stateTrackingVarName = "";

        internal Dictionary<Behavior, string> enumValueNameMap = new();

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
