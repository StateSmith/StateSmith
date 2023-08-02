#nullable enable

using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.IO;
using System.Linq;

namespace StateSmith.Output;

/// <summary>
/// Has ability to summarize a design before and after transformations.
/// The before stage will also include render config nodes at the root of the diagram (outside state machine).
/// https://github.com/StateSmith/StateSmith/issues/200
/// </summary>
public class SmDesignDescriber : IDisposable
{
    protected SmDesignDescriberSettings settings;
    private readonly IStateMachineProvider stateMachineProvider;
    protected IDiagramVerticesProvider diagramVerticesProvider;
    private readonly IOutputInfo outputInfo;
    protected SmGraphDescriber smDescriber;
    protected TextWriter? writer;
    protected bool needsSeparator = false;

    public SmDesignDescriber(SmDesignDescriberSettings settings, IStateMachineProvider stateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider, IOutputInfo outputInfo)
    {
        this.settings = settings;
        this.stateMachineProvider = stateMachineProvider;
        this.diagramVerticesProvider = diagramVerticesProvider;
        this.outputInfo = outputInfo;
        smDescriber = new SmGraphDescriber(new DummyTextWriter());
    }

    public void Prepare()
    {
        if (!settings.enabled)
            return;

        smDescriber.SetOutputAncestorHandlers(settings.outputAncestorHandlers);

        if (writer == null)
        {
            var filePath = $"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.md";
            writer = new StreamWriter(filePath);
            smDescriber.SetTextWriter(writer);
        }
    }

    public void Dispose()
    {
        smDescriber.Dispose();
    }

    public void DescribeBeforeTransformations()
    {
        if (!settings.enabled || !settings.outputSections.beforeTransformations)
            return;
        
        MaybeAddSeparator();
        smDescriber.OutputHeader("BEFORE TRANSFORMATIONS");

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

        needsSeparator = true;
    }

    protected void MaybeAddSeparator()
    {
        if (!needsSeparator)
            return;

        smDescriber.ClearSeparatorFlag();
        AddBigDividingLine();
        needsSeparator = false;
    }

    protected void AddBigDividingLine()
    {
        for (int i = 0; i < 3; i++)
            smDescriber.WriteLine("");

        for (int i = 0; i < 6; i++) 
            smDescriber.Write("<br>\n");

        smDescriber.WriteLine("");
    }

    public void DescribeAfterTransformations()
    {
        if (!settings.enabled || !settings.outputSections.afterTransformations)
            return;

        MaybeAddSeparator();
        smDescriber.OutputHeader("AFTER TRANSFORMATIONS");
        smDescriber.Describe(stateMachineProvider.GetStateMachine());
    }

    internal void SetTextWriter(TextWriter writer)
    {
        this.writer = writer;
        smDescriber.SetTextWriter(writer);
    }
}

