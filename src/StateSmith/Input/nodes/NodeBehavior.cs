#nullable enable

using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public class NodeBehavior
{
    public List<string> triggers = new List<string>();

    public string? order;
    public string guardCode = "";
    public string actionCode = "";
    public string? viaEntry;
    public string? viaExit;
}
