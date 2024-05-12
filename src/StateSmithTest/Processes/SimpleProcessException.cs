using System;
#nullable enable

namespace StateSmithTest.Processes;

public class SimpleProcessException : InvalidOperationException
{
    public SimpleProcessException(string? message) : base(message)
    {
    }
}
