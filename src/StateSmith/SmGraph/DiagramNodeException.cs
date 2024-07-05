using System;
using StateSmith.Input;

namespace StateSmith.SmGraph;

public class DiagramNodeException : Exception
{
    public DiagramNode Node { get; }

    public DiagramNodeException(DiagramNode node) : base()
    {
        Node = node;
    }

    public DiagramNodeException(DiagramNode node, string message) : base(message)
    {
        Node = node;
    }

    public DiagramNodeException(DiagramNode node, string message, Exception innerException) : base(message, innerException)
    {
        Node = node;
    }
}
