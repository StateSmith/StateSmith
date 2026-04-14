using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/538
/// </summary>
public class SubtreeIdMapper
{
    public class SubtreeData
    {
        public int id;
        public NamedVertex vertex;

        public int subtreeEndId;
        public NamedVertex subtreeEndVertex;

        public SubtreeData(NamedVertex vertex, int id)
        {
            this.vertex = vertex;
            this.id = id;
            this.subtreeEndVertex = vertex;
        }
    }

    private NamedVertex? lastVertex;
    private Dictionary<NamedVertex, SubtreeData> map = new();
    private int nextId = 0;

    /// <summary>
    /// NOTE! Only valid to call once per constructed object.
    /// </summary>
    /// <returns></returns>
    public Dictionary<NamedVertex, SubtreeData> MapSubtree(NamedVertex vertex)
    {
        Visit(vertex);
        return map;
    }

    private void Visit(NamedVertex vertex)
    {
        SubtreeData thisVertexIdData = new(vertex, nextId);
        lastVertex = vertex;
        nextId++;
        map.Add(vertex, thisVertexIdData);

        // visit children in order of name to minimize git diffs on user diagram refactoring
        var sortedNamedKids = vertex.NamedChildren().OrderBy(x => x.Name);
        foreach (var child in sortedNamedKids)
        {
            Visit(child);
        }

        // we are now done visiting this subtree.
        // our subtree end id is simply last ID used.
        thisVertexIdData.subtreeEndId = nextId-1;
        thisVertexIdData.subtreeEndVertex = lastVertex;
    }
}
