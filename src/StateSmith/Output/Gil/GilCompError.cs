using System;
using System.Runtime.Serialization;

namespace StateSmith.Output.Gil;

public class GilCompError : Exception
{
    public GilCompError()
    {
    }

    public GilCompError(string message) : base(message)
    {
    }

    public GilCompError(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected GilCompError(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
