using StateSmith.Common;
using StateSmith.Compiling;
using System.Linq;

#nullable enable

namespace StateSmith.compiler;

public class NamedVertexMap
{
    readonly Vertex root;
    public HashList<string, NamedVertex> hashList = new();

    public NamedVertexMap(Vertex root)
    {
        this.root = root;
        UpdateNamedDescendantsMapping();
    }

    public void Reset()
    {
        hashList = new();
    }

    public void UpdateNamedDescendantsMapping()
    {
        Reset();
        root.VisitTypeRecursively<NamedVertex>(vertex => hashList.AddIfMissing(vertex.Name, vertex));
    }

    public State GetState(string name)
    {
        return (State)hashList.GetValues(name).Single();
    }
}
