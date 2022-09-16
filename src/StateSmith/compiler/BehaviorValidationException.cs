using System;

namespace StateSmith.Compiling
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
