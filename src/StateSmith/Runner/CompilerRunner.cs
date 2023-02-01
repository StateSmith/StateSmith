using StateSmith.output.C99BalancedCoder1;
using System;
using System.Collections.Generic;
using System.Linq;
using StateSmith.Compiling;
using System.IO;
using StateSmith.Input;
using StateSmith.Input.PlantUML;
using StateSmith.compiler;
using StateSmith.Input.Yed;
using StateSmith.Input.DrawIo;
using System.Diagnostics.CodeAnalysis;
using StateSmith.Common;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Step 1: compile nodes to vertices
/// Step 2: select the state machine root to compile
/// Step 3: finish running
/// </summary>
public class CompilerRunner
{
    public DiagramToSmConverter diagramToSmConverter = new();
    public Statemachine? sm;

    internal SsServiceProvider ssServiceProvider;
    protected CNameMangler mangler;

    public SmTransformer transformer;

    public CompilerRunner() : this(new SsServiceProvider()) { }

    public CompilerRunner(SsServiceProvider ssServiceProvider)
    {
        this.ssServiceProvider = ssServiceProvider;
        mangler = ssServiceProvider.GetServiceOrCreateInstance();
        transformer = ssServiceProvider.GetServiceOrCreateInstance();
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void CompileFileToVertices(string diagramFile)
    {
        var fileExtension = Path.GetExtension(diagramFile).ToLower();
        FileAssociator fileAssociator = new();

        if (fileAssociator.IsYedExtension(fileExtension))
        {
            CompileYedFileNodesToVertices(diagramFile);
        }
        else if (fileAssociator.IsPlantUmlExtension(fileExtension))
        {
            CompilePlantUmlFileNodesToVertices(diagramFile);
        }
        else if (fileAssociator.IsDrawIoFile(diagramFile)) // needs full diagram file name to support double extension like: `my_file.drawio.svg`
        {
            CompileDrawIoFileNodesToVertices(diagramFile);
        }
        else
        {
            throw new ArgumentException($"Unsupported file extension `{fileExtension}`. \n" + fileAssociator.GetHelpMessage());
        }
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void CompileDrawIoFileNodesToVertices(string filepath)
    {
        DrawIoToSmDiagramConverter converter = ssServiceProvider.GetServiceOrCreateInstance();
        converter.ProcessFile(filepath);
        CompileNodesToVertices(converter.Roots, converter.Edges);
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void CompileYedFileNodesToVertices(string filepath)
    {
        YedParser yedParser = new();
        yedParser.Parse(filepath);
        CompileNodesToVertices(yedParser.GetRootNodes(), yedParser.GetEdges());
    }

    /// <summary>
    /// Step 1
    /// </summary>
    public void CompilePlantUmlFileNodesToVertices(string filepath)
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

        CompileNodesToVertices(new List<DiagramNode> { translator.Root }, translator.Edges);
        FindSingleStateMachine();
    }

    /// <summary>
    /// Step 1. Call this method when you want to support a custom input source.
    /// </summary>
    /// <param name="rootNodes"></param>
    /// <param name="edges"></param>
    public void CompileNodesToVertices(List<DiagramNode> rootNodes, List<DiagramEdge> edges)
    {
        diagramToSmConverter.CompileDiagramNodesEdges(rootNodes, edges);
    }

    /// <summary>
    /// Step 1. Call this method when you already have created a state machine vertex. Probably from testing.
    /// </summary>
    public void SetStateMachineRoot(Statemachine stateMachine)
    {
        diagramToSmConverter.rootVertices = new List<Vertex>() { stateMachine };
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// Step 2
    /// </summary>
    public void FindStateMachineByName(string stateMachineName)
    {
        sm = new Statemachine("non_null_dummy"); // todo_low: figure out how to not need this to appease nullable analysis
        var action = () => { sm = diagramToSmConverter.rootVertices.OfType<Statemachine>().Where(s => s.Name  == stateMachineName).Single(); };
        action.RunOrWrapException((e) => new ArgumentException($"Couldn't find state machine in diagram with name `{stateMachineName}`.", e));
    }

    /// <summary>
    /// Step 2
    /// </summary>
    [MemberNotNull(nameof(sm))]
    public void FindSingleStateMachine()
    {
        sm = new Statemachine("non_null_dummy"); // todo_low: figure out how to not need this to appease nullable analysis. Maybe avoid action below.
        var action = () => { sm = diagramToSmConverter.rootVertices.OfType<Statemachine>().Single(); };
        action.RunOrWrapException((e) => new ArgumentException($"State machine name not specified. Expected diagram to have find 1 Statemachine node at root level. Instead, found {diagramToSmConverter.rootVertices.OfType<Statemachine>().Count()}.", e));
    }

    //------------------------------------------------------------------------

    /// <summary>
    /// Step 3
    /// </summary>
    public void FinishRunningCompiler()
    {
        SetupForSingleSm();
        mangler.SetStateMachine(sm);
        transformer.RunTransformationPipeline(sm);
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
