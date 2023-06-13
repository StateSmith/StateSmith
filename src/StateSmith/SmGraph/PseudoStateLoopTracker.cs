#nullable enable

using StateSmith.SmGraph.Validation;
using System.Collections.Generic;

namespace StateSmith.SmGraph;

/// <summary>
/// Pseudo states are dangerous in that they can create infinite transition loops. This would cause StateSmith to stack overflow if not detected.
/// Instead, we provide a helpful exception.
/// </summary>
public class PseudoStateLoopTracker
{
    private readonly List<Vertex> vertices = new(); // could be made a bit more efficient with a hashmap/dictionary, but the list size should be small.

    public void Push(PseudoStateVertex pseudoState)
    {
        if (vertices.Contains(pseudoState))
        {
            var chainString = "";
            var joiner = "";
            foreach (var vertex in vertices)
            {
                chainString += joiner + Vertex.Describe(vertex);
                joiner = " -> ";
            }

            chainString += joiner + Vertex.Describe(pseudoState);

            throw new VertexValidationException(pseudoState, "transition infinite loop detected: " + chainString);
        }

        vertices.Add(pseudoState);
    }

    public void Pop()
    {
        vertices.RemoveAt(vertices.Count - 1);
    }

    public void Reset()
    {
        vertices.Clear();
    }
}
