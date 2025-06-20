#nullable enable

using System.Collections.Generic;
using StateSmith.SmGraph;
using System.Linq;
using StateSmith.Common;
using System;
using StateSmith.Output.Gil;
using System.Text;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

namespace StateSmith.Output.Algos.Balanced1;

public class AlgoBalanced1 : IGilAlgo
{
    protected readonly AlgoBalanced1Settings settings;
    protected readonly EnumBuilder enumBuilder;
    protected readonly RenderConfigBaseVars renderConfig;
    protected readonly INameMangler mangler;
    protected readonly OutputFile file;
    protected readonly EventHandlerBuilder eventHandlerBuilder;
    protected readonly PseudoStateHandlerBuilder pseudoStateHandlerBuilder;
    protected readonly IAlgoEventIdToString algoEventIdToString;
    protected readonly IAlgoStateIdToString algoStateIdToString;
    protected readonly StandardFileHeaderPrinter standardFileHeaderPrinter;

    protected StateMachine? _sm;
    protected const string AlgoWikiLink = "https://github.com/StateSmith/StateSmith/wiki/Algorithms";

    protected StateMachine Sm => _sm.ThrowIfNull("Must be set before use");

    protected static string ConstMarker => ""; // todo_low - put in an attribute like [ro] that will end up as `const` for languages that support that

    public AlgoBalanced1(INameMangler mangler, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, EnumBuilder enumBuilder, RenderConfigBaseVars renderConfig, EventHandlerBuilder eventHandlerBuilder, CodeStyleSettings styler, AlgoBalanced1Settings settings, IAlgoEventIdToString algoEventIdToString, IAlgoStateIdToString algoStateIdToString, StandardFileHeaderPrinter standardFileHeaderPrinter)
    {
        this.mangler = mangler;
        this.file = new OutputFile(styler, new StringBuilder());
        this.pseudoStateHandlerBuilder = pseudoStateHandlerBuilder;
        this.enumBuilder = enumBuilder;
        this.renderConfig = renderConfig;
        this.eventHandlerBuilder = eventHandlerBuilder;
        this.settings = settings;
        this.algoEventIdToString = algoEventIdToString;
        this.algoStateIdToString = algoStateIdToString;
        this.standardFileHeaderPrinter = standardFileHeaderPrinter;
    }

    public string GenerateGil(StateMachine sm)
    {
        this._sm = sm;
        mangler.SetStateMachine(sm);
        eventHandlerBuilder.SetFile(this.file);

        OutputFileTopAlgoComment();

        file.AppendIndentedLine($"// Generated state machine");
        file.AppendIndented($"public class {mangler.SmTypeName}");

        StartClassBlock();
        GenerateInner();

        GilCreationHelper.AppendGilHelpersFuncs(file);

        EndClassBlock();
        FinalCheck();
        return file.ToString();
    }

    virtual protected void FinalCheck()
    {

    }

    private void OutputFileTopAlgoComment()
    {
        GilCreationHelper.AddFileTopComment(file, standardFileHeaderPrinter.GetFileGilHeader() +
            $"// Algorithm: {GetAlgoName()}. See {AlgoWikiLink}\n");
    }

    virtual protected string GetAlgoName()
    {
        return nameof(AlgorithmId.Balanced1);
    }

    private void EndClassBlock()
    {
        file.FinishCodeBlock();
    }

    private void StartClassBlock()
    {
        file.StartCodeBlock();
    }

    // this is a bit of a hack that helps create the proper indentation for the GIL to C99 step
    private void RunWithPossibleIndentation(Action action)
    {
        if (settings.skipClassIndentation)
            file.DecreaseIndentLevel();

        action();

        if (settings.skipClassIndentation)
            file.IncreaseIndentLevel();
    }

    private void GenerateInner()
    {
        RunWithPossibleIndentation(() =>
        {
            enumBuilder.OutputEventIdCode(file);
            file.AppendIndentedLine();
            enumBuilder.OutputStateIdCode(file);
            file.AppendIndentedLine();

            foreach (var h in Sm.historyStates)
            {
                enumBuilder.OutputHistoryIdCode(file, h);
                file.AppendIndentedLine();
            }

            OutputEventHandlerDelegate();
        });

        pseudoStateHandlerBuilder.output = file;
        pseudoStateHandlerBuilder.mangler = mangler;
        pseudoStateHandlerBuilder.Gather(Sm);
        pseudoStateHandlerBuilder.MapParents();

        OutputStructDefinition();

        RunWithPossibleIndentation(() =>
        {
            OutputFuncCtor();

            OutputFuncStart();
            OutputFuncDispatchEvent();

            OutputExitUpToFunction();

            OutputTriggerHandlers();
            MaybeOutputToStringFunctions();
        });
    }

    protected virtual void OutputEventHandlerDelegate()
    {
        file.AppendIndentedLine($"// event handler type");
        file.AppendIndentedLine($"private delegate void {mangler.SmHandlerFuncType}();");   // todo: use attribute or something to mark delegate as having implicit {mangler.SmTypeName} sm argument?
        file.AppendIndentedLine();
    }

    private void MaybeOutputToStringFunctions()
    {
        if (settings.outputStateIdToStringFunction)
        {
            algoStateIdToString.CreateStateIdToStringFunction(file, Sm);
        }

        if (settings.outputEventIdToStringFunction)
        {
            algoEventIdToString.CreateEventIdToStringFunction(file, Sm);
        }
    }

    internal void OutputStructDefinition()
    {
        file.AppendIndentedLine($"// Used internally by state machine. Feel free to inspect, but don't modify.");
        file.AppendIndentedLine($"public {mangler.SmStateEnumType} {mangler.SmStateIdVarName};");

        OutputAlgoBalanced1StructPointers();

        if (IsVarsStructNeeded())
        {
            RunWithPossibleIndentation(() =>
            {
                file.AppendIndentedLine();
                file.AppendIndentedLine("// State machine variables. Can be used for inputs, outputs, user variables...");
                var type = settings.varsStructAsClass ? "class" : "struct";

                file.AppendIndented($"public {type} Vars");
                file.StartCodeBlock();
                {
                    foreach (var line in StringUtils.SplitIntoLinesOrEmpty(Sm.variables.Trim()))
                    {
                        file.AppendIndentedLine("public " + line);
                    }

                    foreach (var line in StringUtils.SplitIntoLinesOrEmpty(renderConfig.VariableDeclarations.Trim()))
                    {
                        file.AppendIndentedLine(GilCreationHelper.WrapRawCodeAsField(line));
                    }
                }
                file.FinishCodeBlock();
            });

            file.AppendIndentedLine();
            file.AppendIndentedLine("// Variables. Can be used for inputs, outputs, user variables...");
            file.AppendIndentedLine("public Vars vars = new Vars();");
        }
    }

    virtual protected void OutputAlgoBalanced1StructPointers()
    {
        file.AppendIndentedLine();
        file.AppendIndentedLine($"// Used internally by state machine. Don't modify.");
        file.AppendIndentedLine($"private {mangler.SmHandlerFuncType}? {mangler.SmAncestorEventHandlerVarName};");

        file.AppendIndentedLine();
        file.AppendIndentedLine($"// Used internally by state machine. Don't modify.");
        file.AppendIndentedLine($"private readonly {mangler.SmHandlerFuncType}?[] {mangler.SmCurrentEventHandlersVarName} = new {mangler.SmHandlerFuncType}[{mangler.SmEventEnumCount}];");

        file.AppendIndentedLine();
        file.AppendIndentedLine($"// Used internally by state machine. Don't modify.");
        file.AppendIndentedLine($"private {mangler.SmHandlerFuncType}? {mangler.SmCurrentStateExitHandlerVarName};");
    }

    internal bool IsVarsStructNeeded()
    {
        if (Sm.variables.Length > 0)
        {
            return true;
        }

        return IsVariableDeclarationsNonEmpty();
    }

    private bool IsVariableDeclarationsNonEmpty()
    {
        return StringUtils.RemoveCCodeComments(renderConfig.VariableDeclarations).Trim().Length > 0;
    }

    /// <summary>
    /// Ctor is short for constructor
    /// </summary>
    internal void OutputFuncCtor()
    {
        file.AppendIndentedLine();
        file.AppendIndentedLine("// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.");
        file.AppendIndented($"public {mangler.SmTypeName}()");
        file.StartCodeBlock();
        file.FinishCodeBlock();
        file.AppendIndentedLine();
    }

    internal void OutputFuncStart()
    {
        file.AppendIndentedLine("// Starts the state machine. Must be called before dispatching events. Not thread safe.");
        file.AppendIndented($"public void {mangler.SmStartFuncName}()");
        file.StartCodeBlock();
        file.AppendIndentedLine("this.ROOT_enter();");

        var initialState = Sm.Children.OfType<InitialState>().Single();

        var getToInitialStateBehavior = new Behavior(Sm, initialState);

        //var tempSmAccess = eventHandlerBuilder.smAccess;
        //eventHandlerBuilder.smAccess = "this";
        eventHandlerBuilder.OutputTransitionCode(getToInitialStateBehavior, noAncestorHandlesEvent: true, checkForExiting: false);
        //eventHandlerBuilder.smAccess = tempSmAccess;

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }

    virtual protected void OutputExitUpToFunction()
    {
        file.AppendIndentedLine("// This function is used when StateSmith doesn't know what the active leaf state is at");
        file.AppendIndentedLine("// compile time due to sub states or when multiple states need to be exited.");

        string desired_state_exit_handler = mangler.MangleVarName("desired_state_exit_handler");

        file.AppendIndented($"private void {mangler.SmExitUpToFuncName}({ConstMarker}{mangler.SmHandlerFuncType} {desired_state_exit_handler})");
        file.StartCodeBlock();

        file.AppendIndented($"while (this.{mangler.SmCurrentStateExitHandlerVarName} != {desired_state_exit_handler})");
        file.StartCodeBlock();
        {
            file.AppendIndentedLine($"this.{mangler.SmCurrentStateExitHandlerVarName}!();");
        }
        file.FinishCodeBlock(forceNewLine: true);

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }

    virtual protected void OutputFuncDispatchEvent()
    {
        OutputFuncDispatchEventStart(out string eventIdParameterName);

        file.StartCodeBlock();
        {
            string behavior_func = mangler.MangleVarName("behavior_func");
            file.AppendIndentedLine($"{mangler.SmHandlerFuncType}? {behavior_func} = this.{mangler.SmCurrentEventHandlersVarName}[(int){eventIdParameterName}];");
            file.AppendIndentedLine();
            file.AppendIndented($"while ({behavior_func} != null)");
            {
                file.StartCodeBlock();
                file.AppendIndentedLine($"this.{mangler.SmAncestorEventHandlerVarName} = null;");
                file.AppendIndentedLine($"{behavior_func}();");
                file.AppendIndentedLine($"{behavior_func} = this.{mangler.SmAncestorEventHandlerVarName};");
                file.FinishCodeBlock(forceNewLine: true);
            }
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendIndentedLine();
    }

    protected string OutputFuncDispatchEventStart(out string eventIdParameterName)
    {
        eventIdParameterName = mangler.MangleVarName("event_id");

        file.AppendIndentedLine("// Dispatches an event to the state machine. Not thread safe.");
        file.AppendIndentedLine($"// Note! This function assumes that the `{eventIdParameterName}` parameter is valid.");

        file.AppendIndented($"public void {mangler.SmDispatchEventFuncName}({mangler.SmEventEnumType} {eventIdParameterName})");
        return eventIdParameterName;
    }

    internal void OutputTriggerHandlers()
    {
        List<NamedVertex> namedVertices = Sm.GetNamedVerticesCopy();

        foreach (var state in namedVertices)
        {
            eventHandlerBuilder.OutputNamedStateHandlers(state);
        }
    }
}
