using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph.Validation;
using StateSmith.SmGraph.Visitors;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

public class RenderConfigVerticesProcessor : RecursiveVertexVisitor
{
    readonly RenderConfigBaseVars tempRenderConfigBaseVars = new();

    private readonly RenderConfigAllVars allVars;
    private readonly IStateMachineProvider targetStateMachineProvider;
    private readonly IDiagramVerticesProvider diagramVerticesProvider;

    /// <summary>
    /// remember that a diagram can have multiple state machines at the diagram root level.
    /// </summary>
    StateMachine? currentStateMachine = null;

    public RenderConfigVerticesProcessor(RenderConfigAllVars allVars, IStateMachineProvider targetStateMachineProvider, IDiagramVerticesProvider diagramVerticesProvider)
    {
        this.allVars = allVars;
        this.targetStateMachineProvider = targetStateMachineProvider;
        this.diagramVerticesProvider = diagramVerticesProvider;
    }

    // added to support simulator
    internal RenderConfigVerticesProcessor(RenderConfigAllVars allVars, StateMachine sm)
    {
        this.allVars = allVars;
        this.targetStateMachineProvider = new StateMachineProvider(sm);
        this.diagramVerticesProvider = new DiagramVerticesProviderBasic(sm);
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
        allVars.Base.CopyFrom(tempRenderConfigBaseVars);
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
            case nameof(IRenderConfig.VariableDeclarations): AppendOption(ref tempRenderConfigBaseVars.VariableDeclarations, v); break;
            case nameof(IRenderConfig.AutoExpandedVars): AppendOption(ref tempRenderConfigBaseVars.AutoExpandedVars, v); break;
            case nameof(IRenderConfig.DefaultVarExpTemplate): AppendOption(ref tempRenderConfigBaseVars.DefaultVarExpTemplate, v); break;
            case nameof(IRenderConfig.DefaultFuncExpTemplate): AppendOption(ref tempRenderConfigBaseVars.DefaultFuncExpTemplate, v); break;
            case nameof(IRenderConfig.DefaultAnyExpTemplate): AppendOption(ref tempRenderConfigBaseVars.DefaultAnyExpTemplate, v); break;
            case nameof(IRenderConfig.EventCommaList): AppendOption(ref tempRenderConfigBaseVars.EventCommaList, v); break;
            case nameof(IRenderConfig.TriggerMap): AppendOption(ref tempRenderConfigBaseVars.TriggerMap, v); break;
            case nameof(IRenderConfig.FileTop): AppendOption(ref tempRenderConfigBaseVars.FileTop, v); break;

            case nameof(IRenderConfigC.HFileTop): AppendOption(ref allVars.C.HFileTop, v); break;
            case nameof(IRenderConfigC.HFileIncludes): AppendOption(ref allVars.C.HFileIncludes, v); break;
            case nameof(IRenderConfigC.CFileTop): AppendOption(ref allVars.C.CFileTop, v); break;
            case nameof(IRenderConfigC.CFileIncludes): AppendOption(ref allVars.C.CFileIncludes, v); break;
            //
            case nameof(IRenderConfigC.HFileExtension): SetOption(ref allVars.C.HFileExtension, v); break;
            case nameof(IRenderConfigC.CFileExtension): SetOption(ref allVars.C.CFileExtension, v); break;
            case nameof(IRenderConfigC.CEnumDeclarer): SetOption(ref allVars.C.CEnumDeclarer, v); break;
            case nameof(IRenderConfigC.UseStdBool): allVars.C.UseStdBool = ParseBoolValue(v); break;
            case nameof(IRenderConfigC.IncludeGuardLabel): SetOption(ref allVars.C.IncludeGuardLabel, v); break;

            case "CSharp" + nameof(IRenderConfigCSharp.NameSpace): AppendOption(ref allVars.CSharp.NameSpace, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.Usings): AppendOption(ref allVars.CSharp.Usings, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.ClassCode): AppendOption(ref allVars.CSharp.ClassCode, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.BaseList): AppendOption(ref allVars.CSharp.BaseList, v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.UseNullable): allVars.CSharp.UseNullable = ParseBoolValue(v); break;
            case "CSharp" + nameof(IRenderConfigCSharp.UsePartialClass): allVars.CSharp.UsePartialClass = ParseBoolValue(v); break;

            case "JavaScript" + nameof(IRenderConfigJavaScript.ClassCode): AppendOption(ref allVars.JavaScript.ClassCode, v); break;
            case "JavaScript" + nameof(IRenderConfigJavaScript.ExtendsSuperClass): AppendOption(ref allVars.JavaScript.ExtendsSuperClass, v); break;
            case "JavaScript" + nameof(IRenderConfigJavaScript.PrivatePrefix): SetOption(ref allVars.JavaScript.PrivatePrefix, v); break;
            case "JavaScript" + nameof(IRenderConfigJavaScript.UseExportOnClass): allVars.JavaScript.UseExportOnClass = ParseBoolValue(v); break;

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
