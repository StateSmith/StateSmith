#nullable enable

using StateSmith.Common;
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
    protected StringWriter writer = new();
    protected bool needsSeparator = false;

    public SmDesignDescriber(SmDesignDescriberSettings settings, IStateMachineProvider stateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider, IOutputInfo outputInfo)
    {
        this.settings = settings;
        this.stateMachineProvider = stateMachineProvider;
        this.diagramVerticesProvider = diagramVerticesProvider;
        this.outputInfo = outputInfo;
        smDescriber = new SmGraphDescriber(writer);
    }

    public void Prepare()
    {
        if (!settings.enabled)
            return;

        smDescriber.SetOutputAncestorHandlers(settings.outputAncestorHandlers);
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

            smDescriber.DescribeRecursively(v);
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
        smDescriber.DescribeRecursively(stateMachineProvider.GetStateMachine());
    }

    public string GetOutput()
    {
        return writer.GetStringBuilder().ToString();
    }

    public void WriteOutput(ICodeFileWriter fileWriter)
    {
        if (!settings.enabled)
            return;

        var filePath = $"{settings.outputDirectory}{outputInfo.BaseFileName}.md";

        // If the output directory doesn't exist, create it.
        Directory.CreateDirectory(settings.outputDirectory.ThrowIfNull());

        fileWriter.WriteFile(filePath, GetOutput());
    }
}

