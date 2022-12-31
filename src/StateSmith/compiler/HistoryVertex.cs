#nullable enable

using System.Collections.Generic;
using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class HistoryVertex : PseudoStateVertex
    {
        internal string stateTrackingVarName = "";

        internal Dictionary<Behavior, string> enumValueNameMap = new();

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
