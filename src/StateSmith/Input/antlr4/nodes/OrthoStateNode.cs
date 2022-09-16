using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace StateSmith.Input.antlr4
{

    public class OrthoStateNode : StateNode
    {
        public string order;
    }

    public class EntryPointNode : Node
    {
        public string label;
    }

    public class ExitPointNode : Node
    {
        public string label;
    }

}
