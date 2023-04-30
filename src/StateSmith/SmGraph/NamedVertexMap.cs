using StateSmith.Common;
using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

public class NamedVertexMap
{
    readonly Vertex root;
    private readonly bool skipRoot;
    public HashList<string, NamedVertex> hashList = new();

    public NamedVertexMap(Vertex root, bool skipRoot = false)
    {
        this.root = root;
        this.skipRoot = skipRoot;
        UpdateNamedDescendantsMapping();
    }

    private void Reset()
    {
        hashList = new();
    }

    public void UpdateNamedDescendantsMapping()
    {
        Reset();
        root.VisitTypeRecursively<NamedVertex>(vertex =>
        {
            if (skipRoot && root == vertex) return;
            hashList.AddIfValueMissing(vertex.Name, vertex);
        });
    }

    public State GetState(string name)
    {
        return GetStatesWithName(name).Single();
    }

    public List<State> GetStatesWithName(string name)
    {
        return hashList.GetValues(name).OfType<State>().ToList();
    }

    public List<NamedVertex> GetNamedVerticesWithName(string name)
    {
        return hashList.GetValues(name);
    }

    public List<string> GetAllNames()
    {
        return hashList.GetKeys();
    }
}
