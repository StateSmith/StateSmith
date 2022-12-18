#nullable enable

using StateSmith.compiler.Visitors;

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
}
