﻿#nullable enable

using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class HistoryVertex : PseudoStateVertex
    {
        internal string stateTrackingVarName = "";

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}