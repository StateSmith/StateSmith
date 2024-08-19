#nullable enable

using StateSmith.Runner;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/330
/// </summary>
public class SmFileNameProcessor
{
    readonly DiagramFilePathProvider diagramFilePathProvider;

    public SmFileNameProcessor(DiagramFilePathProvider diagramFilePathProvider)
    {
        this.diagramFilePathProvider = diagramFilePathProvider;
    }

    public void ProcessRoots(List<Vertex> rootVertices)
    {
        foreach (var root in rootVertices.OfType<StateMachine>())
        {
            ProcessStateMachine(root);
        }
    }

    public void ProcessStateMachine(StateMachine sm)
    {
        if (sm.Name.Contains('{') || sm.Name.Contains('}'))
        {
            var name = sm.Name.Replace("{fileName}", diagramFilePathProvider.GetDiagramBaseName());

            // replace any invalid characters for C identifiers
            name = new Regex(@"[^a-zA-Z0-9_]").Replace(name, "_");
            sm.Rename(name);
        }
    }
}
