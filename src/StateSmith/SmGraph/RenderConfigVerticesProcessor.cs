using StateSmith.Output.UserConfig;
using StateSmith.SmGraph.Visitors;
using System;

#nullable enable

namespace StateSmith.SmGraph;

public class RenderConfigVerticesProcessor : DummyVertexVisitor
{
    readonly RenderConfigC renderConfigC;
    readonly StateMachine targetStateMachine;
    readonly IDiagramVerticesProvider diagramVerticesProvider;

    /// <summary>
    /// remember that a diagram can have multiple state machines at the diagram root level.
    /// </summary>
    StateMachine? currentStateMachine = null;

    public RenderConfigVerticesProcessor(RenderConfigC renderConfigC, StateMachine targetStateMachine, IDiagramVerticesProvider diagramVerticesProvider)
    {
        this.renderConfigC = renderConfigC;
        this.targetStateMachine = targetStateMachine;
        this.diagramVerticesProvider = diagramVerticesProvider;
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

    public override void Visit(RenderConfigOptionVertex v)
    {
        if (v.Children.Count > 0)
        {
            throw new VertexValidationException(v, $"{nameof(RenderConfigOptionVertex)} are currently not allowed to have children nodes (other than notes).");
        }

        if (v.NonNullParent is not RenderConfigVertex)
        {
            throw new VertexValidationException(v, $"{nameof(RenderConfigOptionVertex)} must have a parent of type {nameof(RenderConfigVertex)}.");
        }

        if (HandlingRootLevelRenderConfig() || targetStateMachine == currentStateMachine)
        {
            CopyRenderConfigOption(v);
        }
    }

    internal void Process()
    {
        var rootVertices = diagramVerticesProvider.GetRootVertices();
        foreach (var rootVertex in rootVertices)
        {
            Visit(rootVertex);
        }
    }

    private void CopyRenderConfigOption(RenderConfigOptionVertex v)
    {
        switch (v.name)
        {
            case nameof(RenderConfigC.HFileTop):
                renderConfigC.HFileTop += v.value;
                break;

            case nameof(RenderConfigC.HFileIncludes):
                renderConfigC.HFileIncludes += v.value;
                break;

            case nameof(RenderConfigC.CFileTop):
                renderConfigC.CFileTop += v.value;
                break;

            case nameof(RenderConfigC.CFileIncludes):
                renderConfigC.CFileIncludes += v.value;
                break;

            case nameof(RenderConfigC.VariableDeclarations):
                renderConfigC.VariableDeclarations += v.value;
                break;

            case nameof(RenderConfigC.EventCommaList):
                renderConfigC.EventCommaList += v.value;
                break;

            default:
                throw new VertexValidationException(v, $"Unknown Render Config option `{v.name}`");
        }
    }

    // applies to any StateMachine in diagram
    private bool HandlingRootLevelRenderConfig()
    {
        return currentStateMachine == null;
    }
}
