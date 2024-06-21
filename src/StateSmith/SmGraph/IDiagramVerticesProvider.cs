using System.Collections.Generic;

#nullable enable

namespace StateSmith.SmGraph;

public interface IDiagramVerticesProvider
{
    List<Vertex> GetRootVertices();
}
