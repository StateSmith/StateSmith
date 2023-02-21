using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

public class RenderConfigVerticesProcessor : DummyVertexVisitor
{
    readonly RenderConfigC tempRenderConfigC = new();
    readonly RenderConfig tempRenderConfig = new();
    private readonly RenderConfig renderConfig;
    readonly RenderConfigC renderConfigC;
    readonly IStateMachineProvider targetStateMachineProvider;
    readonly IDiagramVerticesProvider diagramVerticesProvider;

    /// <summary>
    /// remember that a diagram can have multiple state machines at the diagram root level.
    /// </summary>
    StateMachine? currentStateMachine = null;

    public RenderConfigVerticesProcessor(RenderConfig renderConfig, RenderConfigC renderConfigC, IStateMachineProvider targetStateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider)
    {
        this.renderConfig = renderConfig;
        this.renderConfigC = renderConfigC;
        this.targetStateMachineProvider = targetStateMachineProvider;
        this.diagramVerticesProvider = diagramVerticesProvider;
    }

    /// <summary>
    /// copies RenderConfig data from diagram and then removes those vertices from the state machine.
    /// https://github.com/StateSmith/StateSmith/issues/23
    /// </summary>
    public void Process()
    {
        var rootVertices = diagramVerticesProvider.GetRootVertices();

        // visit diagram root render config vertices first, before any render configs inside state machines
        var visitFirst = rootVertices.OfType<RenderConfigVertex>().ToList();
        visitFirst.ForEach(v => Visit(v));

        foreach (var v in rootVertices.Except(visitFirst))
        {
            Visit(v);
        }
        
        List<Vertex> toRemove = new();
        targetStateMachineProvider.GetStateMachine().VisitTypeRecursively<RenderConfigVertex>(v => toRemove.Add(v));

        foreach (var v in toRemove)
        {
            v.RemoveChildrenAndSelf();
        }

        renderConfigC.CopyFrom(tempRenderConfigC); // done like this so it can be processed intelligently
        renderConfig.CopyFrom(tempRenderConfig); // done like this so it can be processed intelligently
    }

    public override void Visit(StateMachine v)
    {
        currentStateMachine = v;
        base.Visit(v);
        currentStateMachine = null;
    }

    public override void Visit(RenderConfigVertex v)
    {
        if (v.Parent != null && v.Parent is not StateMachine)
        {
            throw new VertexValidationException(v, $"{nameof(RenderConfigVertex)} are currently only allowed at the diagram root, or directly inside a StateMachine.");
        }

        base.Visit(v);
    }

    public override void Visit(ConfigOptionVertex configOption)
    {
        if (configOption.Children.Where(c => c is not NotesVertex).Any())
        {
            throw new VertexValidationException(configOption, $"{nameof(ConfigOptionVertex)} are currently not allowed to have children nodes (other than notes).");
        }

        if (configOption.NonNullParent is not RenderConfigVertex)
        {
            throw new VertexValidationException(configOption, $"{nameof(ConfigOptionVertex)} must have a parent of type {nameof(RenderConfigVertex)}.");
        }

        if (HandlingRootLevelRenderConfig() || targetStateMachineProvider.GetStateMachine() == currentStateMachine)
        {
            CopyRenderConfigOption(configOption);
        }
    }

    private void CopyRenderConfigOption(ConfigOptionVertex v)
    {
        switch (v.name)
        {
            case nameof(RenderConfig.VariableDeclarations): AppendOption(ref tempRenderConfig.VariableDeclarations, v); break;
            case nameof(RenderConfig.AutoExpandedVars): AppendOption(ref tempRenderConfig.AutoExpandedVars, v); break;
            case nameof(RenderConfig.EventCommaList): AppendOption(ref tempRenderConfig.EventCommaList, v); break;
            case nameof(RenderConfig.FileTop): AppendOption(ref tempRenderConfigC.FileTop, v); break;

            case nameof(RenderConfigC.HFileTop): AppendOption(ref tempRenderConfigC.HFileTop, v); break;
            case nameof(RenderConfigC.HFileIncludes): AppendOption(ref tempRenderConfigC.HFileIncludes, v); break;
            case nameof(RenderConfigC.CFileTop): AppendOption(ref tempRenderConfigC.CFileTop, v); break;
            case nameof(RenderConfigC.CFileIncludes): AppendOption(ref tempRenderConfigC.CFileIncludes, v); break;

            default:
                throw new VertexValidationException(v, $"Unknown Render Config option `{v.name}`");
        }
    }

    private static void AppendOption(ref string str, ConfigOptionVertex option)
    {
        var toAppend = option.value;
        str = StringUtils.AppendWithNewlineIfNeeded(str, toAppend);
    }

    // applies to any StateMachine in diagram
    private bool HandlingRootLevelRenderConfig()
    {
        return currentStateMachine == null;
    }
}
