using StateSmith.compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace StateSmith.Compiling
{
    public class Behavior
    {
        public const double DEFAULT_ORDER = 1e6;

        /// <summary>
        /// Only populated for transitions.
        /// </summary>
        public string? DiagramId { get; set; }

        internal Vertex _owningVertex;
        public Vertex OwningVertex => _owningVertex;

        /// <summary>
        /// Allowed to be null
        /// </summary>
        internal Vertex? _transitionTarget;

        public Vertex? TransitionTarget => _transitionTarget;

        public List<string> triggers = new List<string>();
        public double order = DEFAULT_ORDER;
        public string? guardCode;
        public string? actionCode;

        // https://github.com/StateSmith/StateSmith/issues/3
        public string? viaEntry;

        // https://github.com/StateSmith/StateSmith/issues/3
        public string? viaExit;

        public Behavior() { }

        public Behavior(Vertex owningVertex, Vertex? transitionTarget = null)
        {
            _owningVertex = owningVertex;

            if (transitionTarget != null)
            {
                _transitionTarget = transitionTarget;
                transitionTarget.AddIncomingTransition(this);
            }
        }

        [MemberNotNullWhen(true, nameof(TransitionTarget))]
        public bool HasTransition()
        {
            return TransitionTarget != null;
        }

        [MemberNotNullWhen(true, nameof(guardCode))]
        public bool HasGuardCode()
        {
            return IsCodePresent(guardCode);
        }

        [MemberNotNullWhen(true, nameof(actionCode))]
        public bool HasActionCode()
        {
            return IsCodePresent(actionCode);
        }

        private static bool IsCodePresent(string? code)
        {
            return code != null && code.Trim().Length > 0;  //trim not ideal for performance, but fine for now
        }

        public bool HasAtLeastOneTrigger()
        {
            return triggers.Count > 0;
        }

        /// <summary>
        /// Must have had an original target
        /// </summary>
        /// <param name="newTarget"></param>
        public void RetargetTo(Vertex newTarget)
        {
            var oldTarget = TransitionTarget;
            oldTarget!.RemoveIncomingTransition(this);

            _transitionTarget = newTarget;
            newTarget.AddIncomingTransition(this);
        }

        /// <summary>
        /// Throws if no transition target
        /// </summary>
        /// <returns></returns>
        public TransitionPath FindTransitionPath()
        {
            return OwningVertex.FindTransitionPathTo(TransitionTarget);
        }
    }
}
