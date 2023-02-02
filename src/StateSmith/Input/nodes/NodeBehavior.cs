using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4
{

    public class NodeBehavior
    {
        public List<string> triggers = new List<string>();

        /// <summary>
        /// Allowed to be null!
        /// </summary>
        public string order;
        public string guardCode;
        public string actionCode;
        public string viaEntry;
        public string viaExit;
    }
}
