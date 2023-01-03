using System;

namespace StateSmith.Input.DrawIo;

public class DrawIoException : Exception
{
    public DrawIoException() { }
    public DrawIoException(string message) : base(message) { }
    public DrawIoException(string message, Exception inner) : base(message, inner) { }
}