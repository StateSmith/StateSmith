using System;
using StateSmith.Input;

namespace StateSmith.SmGraph;

public class DiagramNodeParseException : DiagramNodeException
{
    public DiagramNodeParseException(DiagramNode node) : base(node)
    {
    }

    public DiagramNodeParseException(DiagramNode node, string message) : base(node, message)
    {
    }

    public DiagramNodeParseException(DiagramNode node, string message, Exception innerException) : base(node, message, innerException)
    {
    }
}
