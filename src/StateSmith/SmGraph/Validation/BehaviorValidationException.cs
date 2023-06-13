#nullable enable
using System;

namespace StateSmith.SmGraph.Validation
{
    public class BehaviorValidationException : Exception
    {
        public Behavior behavior;

        public BehaviorValidationException(Behavior behavior, string message) : base(message)
        {
            this.behavior = behavior;
        }
    }
}
