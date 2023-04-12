#nullable enable
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public class StateMachineNode : Node, INodeWithBehaviors
{
    public string? name;
    public List<NodeBehavior> Behaviors { get; set; } = new();
}
