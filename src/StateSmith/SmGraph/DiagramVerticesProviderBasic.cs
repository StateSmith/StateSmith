#nullable enable
using System.Collections.Generic;
namespace StateSmith.SmGraph;

internal class DiagramVerticesProviderBasic : IDiagramVerticesProvider
{
    List<Vertex> rootVertices;

    public DiagramVerticesProviderBasic(StateMachine sm)
    {
        rootVertices = new() { sm };
    }

    public List<Vertex> GetRootVertices()
    {
        return rootVertices;
    }
}
