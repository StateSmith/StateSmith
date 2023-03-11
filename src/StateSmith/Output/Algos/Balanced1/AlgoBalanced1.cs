#nullable enable

using System.Collections.Generic;
using StateSmith.SmGraph;
using System.Linq;
using StateSmith.Common;
using System;
using StateSmith.Output.Gil;
using System.Text;
using StateSmith.Output.UserConfig;

namespace StateSmith.Output.Algos.Balanced1;

public class AlgoBalanced1 : IGilAlgo
{
    protected readonly AlgoBalanced1Settings settings;
    protected readonly EnumBuilder enumBuilder;
    protected readonly RenderConfigVars renderConfig;
    protected readonly NameMangler mangler;
    protected readonly OutputFile file;
    protected readonly EventHandlerBuilder eventHandlerBuilder;
    protected readonly PseudoStateHandlerBuilder pseudoStateHandlerBuilder;
    protected readonly IAlgoEventIdToString algoEventIdToString;
    protected readonly IAlgoStateIdToString algoStateIdToString;

    protected StateMachine? _sm;
    protected StateMachine Sm => _sm.ThrowIfNull("Must be set before use");

    string ConstMarker => ""; // todo_low - put in an attribute like [ro] that will end up as `const` for languages that support that

    public AlgoBalanced1(NameMangler mangler, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, EnumBuilder enumBuilder, RenderConfigVars renderConfig, EventHandlerBuilder eventHandlerBuilder, CodeStyleSettings styler, AlgoBalanced1Settings settings, IAlgoEventIdToString algoEventIdToString, IAlgoStateIdToString algoStateIdToString)
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
    }

    public string GenerateGil(StateMachine sm)
    {
        this._sm = sm;
        mangler.SetStateMachine(sm);
        eventHandlerBuilder.SetFile(this.file);
        file.AppendLine($"// Generated state machine");
        file.Append($"public class {mangler.SmTypeName}");

        StartClassBlock();
        GenerateInner();

        GilHelper.AppendGilHelpersFuncs(file);

        EndClassBlock();
        return file.ToString();
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
            file.AppendLine();
            enumBuilder.OutputStateIdCode(file);
            file.AppendLine();

            foreach (var h in Sm.historyStates)
            {
                enumBuilder.OutputHistoryIdCode(file, h);
                file.AppendLine();
            }

            file.AppendLine($"// event handler type");
            file.AppendLine($"private delegate void {mangler.SmHandlerFuncType}();");   // todo: use attribute or something to mark delegate as having implicit {mangler.SmTypeName} sm argument?
            file.AppendLine();
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

    private void MaybeOutputToStringFunctions()
    {
        if (settings.generateStateIdToStringFunction)
        {
            algoStateIdToString.CreateStateIdToStringFunction(file, Sm);
        }

        if (settings.generateEventIdToStringFunction)
        {
            algoEventIdToString.CreateEventIdToStringFunction(file, Sm);
        }
    }

    internal void OutputStructDefinition()
    {
        file.AppendLine($"// Used internally by state machine. Feel free to inspect, but don't modify.");
        file.AppendLine($"public {mangler.SmStateEnumType} {mangler.SmStateIdVarName};");

        file.AppendLine();
        file.AppendLine($"// Used internally by state machine. Don't modify.");
        file.AppendLine($"private {mangler.SmHandlerFuncType}? {mangler.SmAncestorEventHandlerVarName};");

        file.AppendLine();
        file.AppendLine($"// Used internally by state machine. Don't modify.");
        file.AppendLine($"private readonly {mangler.SmHandlerFuncType}?[] {mangler.SmCurrentEventHandlersVarName} = new {mangler.SmHandlerFuncType}[{mangler.SmEventEnumCount}];");

        file.AppendLine();
        file.AppendLine($"// Used internally by state machine. Don't modify.");
        file.AppendLine($"private {mangler.SmHandlerFuncType}? {mangler.SmCurrentStateExitHandlerVarName};");

        if (IsVarsStructNeeded())
        {
            RunWithPossibleIndentation(() =>
            {
                file.AppendLine();
                file.AppendLine("// State machine variables. Can be used for inputs, outputs, user variables...");
                file.Append("public struct Vars");
                file.StartCodeBlock();
                {
                    foreach (var line in StringUtils.SplitIntoLinesOrEmpty(Sm.variables.Trim()))
                    {
                        file.AppendLine("public " + line);
                    }

                    foreach (var line in StringUtils.SplitIntoLinesOrEmpty(renderConfig.VariableDeclarations.Trim()))
                    {
                        file.AppendLine(PostProcessor.echoLineMarker + line);
                    }
                }
                file.FinishCodeBlock();
            });

            file.AppendLine();
            file.AppendLine("// Variables. Can be used for inputs, outputs, user variables...");
            file.AppendLine("public Vars vars;");
        }
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
        file.AppendLine();
        file.AppendLine("// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.");
        file.Append($"public {mangler.SmTypeName}()");
        file.StartCodeBlock();
        file.FinishCodeBlock();
        file.AppendLine();
    }

    internal void OutputFuncStart()
    {
        file.AppendLine("// Starts the state machine. Must be called before dispatching events. Not thread safe.");
        file.Append($"public void {mangler.SmStartFuncName}()");
        file.StartCodeBlock();
        file.AppendLine("ROOT_enter();");

        var initialState = Sm.Children.OfType<InitialState>().Single();

        var getToInitialStateBehavior = new Behavior(Sm, initialState);

        //var tempSmAccess = eventHandlerBuilder.smAccess;
        //eventHandlerBuilder.smAccess = "this";
        eventHandlerBuilder.OutputTransitionCode(getToInitialStateBehavior, noAncestorHandlesEvent: true, checkForExiting: false);
        //eventHandlerBuilder.smAccess = tempSmAccess;

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    internal void OutputExitUpToFunction()
    {
        file.AppendLine("// This function is used when StateSmith doesn't know what the active leaf state is at");
        file.AppendLine("// compile time due to sub states or when multiple states need to be exited.");

        string desired_state_exit_handler = mangler.MangleVarName("desired_state_exit_handler");

        file.Append($"private void {mangler.SmExitUpToFuncName}({ConstMarker}{mangler.SmHandlerFuncType} {desired_state_exit_handler})");
        file.StartCodeBlock();

        file.Append($"while (this.{mangler.SmCurrentStateExitHandlerVarName} != {desired_state_exit_handler})");
        file.StartCodeBlock();
        {
            file.AppendLine($"this.{mangler.SmCurrentStateExitHandlerVarName}!();");
        }
        file.FinishCodeBlock(forceNewLine: true);

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    internal void OutputFuncDispatchEvent()
    {
        file.AppendLine("// Dispatches an event to the state machine. Not thread safe.");
        string event_id = mangler.MangleVarName("event_id");
        string behavior_func = mangler.MangleVarName("behavior_func");

        file.Append($"public void {mangler.SmDispatchEventFuncName}({mangler.SmEventEnumType} {event_id})");
        file.StartCodeBlock();
        file.AppendLine($"{mangler.SmHandlerFuncType}? {behavior_func} = this.{mangler.SmCurrentEventHandlersVarName}[(int){event_id}];");
        file.AppendLine();
        file.Append($"while ({behavior_func} != null)");
        {
            file.StartCodeBlock();
            file.AppendLine($"this.{mangler.SmAncestorEventHandlerVarName} = null;");
            file.AppendLine($"{behavior_func}();");
            file.AppendLine($"{behavior_func} = this.{mangler.SmAncestorEventHandlerVarName};");
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
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
