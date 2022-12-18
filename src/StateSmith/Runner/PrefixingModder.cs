using StateSmith.compiler.Visitors;
using StateSmith.Compiling;
using System.Collections.Generic;

namespace StateSmith.Runner
{
    public class PrefixingModder : OnlyVertexVisitor
    {
        private Stack<string> prefixStack = new();

        public PrefixingModder()
        {
            prefixStack.Push(""); // dummy
        }

        public override void Visit(Vertex v)
        {
            if (v is NamedVertex namedVertex)
            {
                HandleNamedVertex(namedVertex);
            }
        }

        private void HandleNamedVertex(NamedVertex state)
        {
            var activePrefix = prefixStack.Peek();
            state.Rename($"{activePrefix}{state.Name}");    // may rename to the same if no prefix
            
            string foundPrefix = GetPrefixOrBlank(state);

            prefixStack.Push(foundPrefix);
            VisitChildren(state);
            prefixStack.Pop();
        }

        private static string GetPrefixOrBlank(NamedVertex state)
        {
            string prefix = "";

            for (int i = 0; i < state.Behaviors.Count; i++) // non-iterator so we can modify behaviors in the loop
            {
                var b = state.Behaviors[i];

                if (b.actionCode.Contains("special_custom_add_prefix_command"))
                {
                    state.RemoveBehaviorAndUnlink(b);
                    prefix = state.Name + "__";
                    break;
                }
            }

            return prefix;
        }
    }
}
