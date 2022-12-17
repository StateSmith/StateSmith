
using StateSmith.compiler.Visitors;
using StateSmith.output.C99BalancedCoder1;
using StateSmith.compiler;
using System.Linq;

namespace StateSmith.Compiling
{
    public class PseudoLoopDetector
    {
        private readonly PseudoStateLoopTracker pseudoStateLoopTracker = new();

        /// <summary>
        /// Prefer this over <see cref="CheckSingleBehavior(Behavior)"/> to have more slightly more helpful error messages
        /// because the exception chain goes like `c1->c2->c3->c1` instead of `c2->c3->c1->c2`.
        /// </summary>
        /// <param name="v"></param>
        public void CheckVertexBehaviors(Vertex v)
        {
            if (v is PseudoStateVertex pseudoState)
            {
                pseudoStateLoopTracker.Push(pseudoState);

                foreach (var b in v.Behaviors)
                {
                    CheckSingleBehavior(b);
                }

                pseudoStateLoopTracker.Pop();
            }
        }

        public void CheckSingleBehavior(Behavior b)
        {
            if (!b.HasTransition())
            {
                return;
            }

            var target = b.TransitionTarget;

            if (target is PseudoStateVertex pseudoStateVertex)
            {
                CheckVertexBehaviors(pseudoStateVertex);
            }
            else if (target.HasInitialState(out var initialState))
            {
                CheckVertexBehaviors(initialState);
            }
        }
    }
}
