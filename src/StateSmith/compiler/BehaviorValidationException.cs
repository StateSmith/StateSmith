using System;

namespace StateSmith.Compiling
{
    public class BehaviorValidationException : Exception
    {
        public BehaviorValidationException(Behavior behavior, string message) : base(message)
        {
        }
    }
}
