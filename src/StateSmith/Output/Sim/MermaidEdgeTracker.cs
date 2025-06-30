using StateSmith.SmGraph;
using System.Collections.Generic;

namespace StateSmith.Output.Sim;

/// <summary>
/// This class maps a behavior to its corresponding mermaid edge ID.
/// </summary>
public class MermaidEdgeTracker
{
    Dictionary<Behavior, int> edgeIdMap = new();
    int nextId = 0;

    public int AddEdge(Behavior b)
    {
        int id = nextId;
        AdvanceId();
        edgeIdMap.Add(b, id);
        return id;
    }

    // use when a non-behavior edge is added
    public int AdvanceId()
    {
        return nextId++;
    }

    public bool ContainsEdge(Behavior b)
    {
        return edgeIdMap.ContainsKey(b);
    }

    public int GetEdgeId(Behavior b)
    {
        return edgeIdMap[b];
    }
}
