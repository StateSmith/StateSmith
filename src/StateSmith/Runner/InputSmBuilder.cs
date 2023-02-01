using StateSmith.output.C99BalancedCoder1;
using System;
using System.Collections.Generic;
using System.Linq;
using StateSmith.SmGraph;
using System.IO;
using StateSmith.Input;
using StateSmith.Input.PlantUML;
using StateSmith.Input.Yed;
using StateSmith.Input.DrawIo;
using System.Diagnostics.CodeAnalysis;
using StateSmith.Common;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// This class converts an input diagram/design into a StateMachine vertex and finishes building/transforming
/// the StateMachine vertex so that is ready for code generation.
/// 
/// Step 1: create StateMachine vertex (or more) from input (like a diagram). 
/// Some diagram types (other than PlantUML) can have multiple StateMachines.
/// 
/// Step 2: select the StateMachine to build. Optional if input only has a single state machine design.
/// 
/// Step 3: finish building/transforming the selected StateMachine vertex so that is ready for code generation.
/// </summary>
public class InputSmBuilder
{
    public readonly SmTransformer transformer;
    public StateMachine Sm => sm.ThrowIfNull();

    protected StateMachine? sm;
    internal DiagramToSmConverter diagramToSmConverter = new();
    internal SsServiceProvider ssServiceProvider;
    protected CNameMangler mangler;

    public InputSmBuilder() : this(new SsServiceProvider()) { }

    public InputSmBuilder(SsServiceProvider ssServiceProvider)
    {
        this.ssServiceProvider = ssServiceProvider;
        mangler = ssServiceProvider.GetServiceOrCreateInstance();
        transformer = ssServiceProvider.GetServiceOrCreateInstance();
    }

    /// <summary>
    /// Step 1. Figures out how to parse file based on file name.
    /// </summary>
    public void ConvertDiagramFileToSmVertices(string diagramFile)
    {
        var fileExtension = Path.GetExtension(diagramFile).ToLower();
        FileAssociator fileAssociator = new();

        if (fileAssociator.IsYedExtension(fileExtension))
        {
            ConvertYedFileNodesToVertices(diagramFile);
        }
        else if (fileAssociator.IsPlantUmlExtension(fileExtension))
        {
            ConvertPlantUmlFileNodesToVertices(diagramFile);
        }
        else if (fileAssociator.IsDrawIoFile(diagramFile)) // needs full diagram file name to support double extension like: `my_file.drawio.svg`
        {
            ConvertDrawIoFileNodesToVertices(diagramFile);
        }
        else
        {
            throw new ArgumentException($"Unsupported file extension `{fileExtension}`. \n" + fileAssociator.GetHelpMessage());
        }
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void ConvertDrawIoFileNodesToVertices(string filepath)
    {
        DrawIoToSmDiagramConverter converter = ssServiceProvider.GetServiceOrCreateInstance();
        converter.ProcessFile(filepath);
        ConvertNodesToVertices(converter.Roots, converter.Edges);
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void ConvertYedFileNodesToVertices(string filepath)
    {
        YedParser yedParser = new();
        yedParser.Parse(filepath);
        ConvertNodesToVertices(yedParser.GetRootNodes(), yedParser.GetEdges());
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void ConvertPlantUmlFileNodesToVertices(string filepath)
    {
        var text = File.ReadAllText(filepath);
        CompilePlantUmlTextNodesToVertices(text);
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void CompilePlantUmlTextNodesToVertices(string plantUmlText)
    {
        PlantUMLToNodesEdges translator = new();
        translator.ParseDiagramText(plantUmlText);

        if (translator.HasError())
        {
            string reasons = Input.antlr4.AntlrError.ErrorsToReasonStrings(translator.GetErrors(), separator: "\n  - ");
            throw new FormatException("PlantUML input failed parsing. Reason(s):\n  - " + reasons);
        }

        ConvertNodesToVertices(new List<DiagramNode> { translator.Root }, translator.Edges);
        FindSingleStateMachine();
    }

    /// <summary>
    /// Step 1. Call this method when you want to support a custom input source.
    /// </summary>
    /// <param name="rootNodes"></param>
    /// <param name="edges"></param>
    public void ConvertNodesToVertices(List<DiagramNode> rootNodes, List<DiagramEdge> edges)
    {
        diagramToSmConverter.CompileDiagramNodesEdges(rootNodes, edges);
    }

    /// <summary>
    /// Step 1. Call this method when you already have created a state machine vertex. Probably from testing.
    /// </summary>
    public void SetStateMachineRoot(StateMachine stateMachine)
    {
        diagramToSmConverter.rootVertices = new List<Vertex>() { stateMachine };
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// Step 2
    /// </summary>
    public void FindStateMachineByName(string stateMachineName)
    {
        sm = new StateMachine("non_null_dummy"); // todo_low: figure out how to not need this to appease nullable analysis
        var action = () => { sm = diagramToSmConverter.rootVertices.OfType<StateMachine>().Where(s => s.Name  == stateMachineName).Single(); };
        action.RunOrWrapException((e) => new ArgumentException($"Couldn't find state machine in diagram with name `{stateMachineName}`.", e));
    }

    /// <summary>
    /// Step 2
    /// </summary>
    [MemberNotNull(nameof(sm))]
    public void FindSingleStateMachine()
    {
        sm = new StateMachine("non_null_dummy"); // todo_low: figure out how to not need this to appease nullable analysis. Maybe avoid action below.
        var action = () => { sm = diagramToSmConverter.rootVertices.OfType<StateMachine>().Single(); };
        action.RunOrWrapException((e) => new ArgumentException($"State machine name not specified. Expected diagram to have find 1 Statemachine node at root level. Instead, found {diagramToSmConverter.rootVertices.OfType<StateMachine>().Count()}.", e));
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// Step 3
    /// </summary>
    public void FinishRunningCompiler()
    {
        SetupForSingleSm();
        mangler.SetStateMachine(Sm);
        transformer.RunTransformationPipeline(Sm);
    }

    [MemberNotNull(nameof(sm))]
    public void SetupForSingleSm()
    {
        if (sm == null)
        {
            FindSingleStateMachine();
        }
    }
}
