#nullable enable

using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.IO;
using System.Linq;

namespace StateSmith.Output;

public class SmDesignDescriber : IDisposable
{
    protected SmDesignDescriberSettings settings;
    private readonly IStateMachineProvider stateMachineProvider;
    protected IDiagramVerticesProvider diagramVerticesProvider;
    private readonly IOutputInfo outputInfo;
    protected SmGraphDescriber smDescriber;
    protected TextWriter? writer;

    public SmDesignDescriber(SmDesignDescriberSettings settings, IStateMachineProvider stateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider, IOutputInfo outputInfo)
    {
        this.settings = settings;
        this.stateMachineProvider = stateMachineProvider;
        this.diagramVerticesProvider = diagramVerticesProvider;
        this.outputInfo = outputInfo;
        smDescriber = new SmGraphDescriber(new DummyTextWriter());
    }

    public void DefaultPrepareToWriteToFile()
    {
        if (!settings.enabled)
            return;

        if (writer != null)
            return;

        var filePath = $"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.md";
        writer = new StreamWriter(filePath);
        smDescriber.SetTextWriter(writer);
    }

    public void Dispose()
    {
        // Note: this doesn't appear to be getting called by dependency injection. Not a big deal with current use though.
        // See https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
        smDescriber.Dispose();
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
        this.writer = writer;
        smDescriber.SetTextWriter(writer);
    }
}

