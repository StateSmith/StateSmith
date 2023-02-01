using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph
{
    public class TransitionPath
    {
        public List<Vertex> toExit = new();
        public Vertex? leastCommonAncestor;
        public List<Vertex> toEnter = new();

        public bool IsSelfTransition()
        {
            if (toExit.Count != 1 || toEnter.Count != 1)
            {
                return false;
            }

            var enterState = toEnter.First();
            var exitState = toExit.First();
            return enterState == exitState;
        }
    }

}
