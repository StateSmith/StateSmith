#nullable enable
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public interface INodeWithBehaviors
{
    List<NodeBehavior> Behaviors { get; set; }
}


