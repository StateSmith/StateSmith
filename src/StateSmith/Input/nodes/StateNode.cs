#nullable enable
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public class StateNode : Node, INodeWithBehaviors
{
    public string? stateName;
    public bool stateNameIsGlobal = false;
    public List<NodeBehavior> Behaviors { get; set; } = new();
}


