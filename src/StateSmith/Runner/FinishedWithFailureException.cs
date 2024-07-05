#nullable enable

using System;

namespace StateSmith.Runner;

public class FinishedWithFailureException : Exception
{
    public FinishedWithFailureException() : base() { }
}
