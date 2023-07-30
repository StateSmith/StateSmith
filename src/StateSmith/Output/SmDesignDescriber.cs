#nullable enable

using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;
using System.Linq;

namespace StateSmith.Output;

public class SmDesignDescriber
{
    protected SmDesignDescriberSettings settings;
    private readonly IStateMachineProvider stateMachineProvider;
    protected IDiagramVerticesProvider diagramVerticesProvider;
    protected SmGraphDescriber smDescriber;

    public SmDesignDescriber(SmDesignDescriberSettings settings, IStateMachineProvider stateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider, IOutputInfo outputInfo)
    {
        this.settings = settings;
        this.stateMachineProvider = stateMachineProvider;
        this.diagramVerticesProvider = diagramVerticesProvider;

        var filePath = $"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.md";
        using var writer = new StreamWriter(filePath);
        smDescriber = new SmGraphDescriber(writer);
    }

    public void DescribeBeforeTransformations()
    {
        if (!settings.enabled)
            return;

        smDescriber.OutputHeader("Before transformations");

        // Sort by diagram ID to keep consistent
        var sortedRootVertices = diagramVerticesProvider.GetRootVertices().OrderBy(v => v.DiagramId).ToList();
        foreach (var v in sortedRootVertices)
        {
            // Skip any other state machines in diagram file.
            // Done this way so that we still print root vertices that are notes and render configs.
            if (v is StateMachine sm && sm != stateMachineProvider.GetStateMachine())
                continue;

            smDescriber.Describe(v);
        }
    }

    public void DescribeAfterTransformations()
    {
        if (!settings.enabled)
            return;

        smDescriber.WriteLine("\n\n\n\n");
        smDescriber.OutputHeader("After transformations");
        smDescriber.Describe(stateMachineProvider.GetStateMachine());
    }

    internal void SetTextWriter(TextWriter writer)
    {
        smDescriber.SetTextWriter(writer);
    }
}

