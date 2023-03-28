using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph.Visitors;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

public class RenderConfigVerticesProcessor : DummyVertexVisitor
{
    readonly RenderConfigVars tempRenderConfigVars = new();

    private readonly RenderConfigVars renderConfigVars;
    private readonly RenderConfigCVars renderConfigCVars;
    private readonly RenderConfigCSharpVars renderConfigCSharpVars;
    private readonly RenderConfigJavaScriptVars renderConfigJavaScriptVars;
    private readonly IStateMachineProvider targetStateMachineProvider;
    private readonly IDiagramVerticesProvider diagramVerticesProvider;

    /// <summary>
    /// remember that a diagram can have multiple state machines at the diagram root level.
    /// </summary>
    StateMachine? currentStateMachine = null;

    public RenderConfigVerticesProcessor(RenderConfigVars renderConfig, RenderConfigCVars renderConfigC, IStateMachineProvider targetStateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider, RenderConfigCSharpVars renderConfigCSharpVars, RenderConfigJavaScriptVars renderConfigJavaScriptVars)
    {
        this.renderConfigVars = renderConfig;
        this.renderConfigCVars = renderConfigC;
        this.renderConfigCSharpVars = renderConfigCSharpVars;
        this.renderConfigJavaScriptVars = renderConfigJavaScriptVars;

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

        // done like this so they can be processed intelligently
        renderConfigVars.CopyFrom(tempRenderConfigVars);
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
            case nameof(IRenderConfig.VariableDeclarations): AppendOption(ref tempRenderConfigVars.VariableDeclarations, v); break;
            case nameof(IRenderConfig.AutoExpandedVars): AppendOption(ref tempRenderConfigVars.AutoExpandedVars, v); break;
            case nameof(IRenderConfig.EventCommaList): AppendOption(ref tempRenderConfigVars.EventCommaList, v); break;
            case nameof(IRenderConfig.FileTop): AppendOption(ref tempRenderConfigVars.FileTop, v); break;

            case nameof(IRenderConfigC.HFileTop): AppendOption(ref renderConfigCVars.HFileTop, v); break;
            case nameof(IRenderConfigC.HFileIncludes): AppendOption(ref renderConfigCVars.HFileIncludes, v); break;
            case nameof(IRenderConfigC.CFileTop): AppendOption(ref renderConfigCVars.CFileTop, v); break;
            case nameof(IRenderConfigC.CFileIncludes): AppendOption(ref renderConfigCVars.CFileIncludes, v); break;

            case "CSharp" + nameof(IRenderConfigCSharp.NameSpace): AppendOption(ref renderConfigCSharpVars.NameSpace, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.Usings): AppendOption(ref renderConfigCSharpVars.Usings, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.ClassCode): AppendOption(ref renderConfigCSharpVars.ClassCode, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.BaseList): AppendOption(ref renderConfigCSharpVars.BaseList, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.UseNullable): renderConfigCSharpVars.UseNullable = ParseBoolValue(v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.UsePartialClass): renderConfigCSharpVars.UsePartialClass = ParseBoolValue(v); break;

            case "JavaScript" + nameof(IRenderConfigJavaScript.ClassCode): AppendOption(ref renderConfigJavaScriptVars.ClassCode, v); break;
            case "JavaScript" + nameof(IRenderConfigJavaScript.ExtendsSuperClass): AppendOption(ref renderConfigJavaScriptVars.ExtendsSuperClass, v); break;
            case "JavaScript" + nameof(IRenderConfigJavaScript.PrivatePrefix): SetOption(ref renderConfigJavaScriptVars.PrivatePrefix, v); break;
            case "JavaScript" + nameof(IRenderConfigJavaScript.UseExportOnClass): renderConfigJavaScriptVars.UseExportOnClass = ParseBoolValue(v); break;

            default:
                throw new VertexValidationException(v, $"Unknown Render Config option `{v.name}`");
        }
    }

    private static bool ParseBoolValue(ConfigOptionVertex v)
    {
        try
        {
            return bool.Parse(v.value.Trim());
        }
        catch (System.FormatException e)
        {
            throw new VertexValidationException(v, e.Message);
        }
    }

    private static void AppendOption(ref string str, ConfigOptionVertex option)
    {
        var toAppend = option.value;
        str = StringUtils.AppendWithNewlineIfNeeded(str, toAppend);
    }

    private static void SetOption(ref string str, ConfigOptionVertex option)
    {
        str = option.value;
    }

    // applies to any StateMachine in diagram
    private bool HandlingRootLevelRenderConfig()
    {
        return currentStateMachine == null;
    }
}
