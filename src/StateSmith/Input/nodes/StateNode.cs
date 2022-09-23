using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace StateSmith.Input.antlr4
{

    public class StateNode : Node
    {
        public string stateName;
        public bool stateNameIsGlobal = false;
        public List<NodeBehavior> behaviors = new List<NodeBehavior>();
    }
}
