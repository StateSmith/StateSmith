#nullable enable

using System;

namespace StateSmith.Runner;

/// <summary>
/// This exception is specifically caught by ss.cli.
/// </summary>
public class FinishedWithFailureException : Exception
{
    /// <summary>
    /// This exception is specifically caught by ss.cli.
    /// </summary>
    public FinishedWithFailureException() : base() { }
}
