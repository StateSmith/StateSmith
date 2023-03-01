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

// Useful info: https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support

public class AlgoBalanced1 : IGilAlgo
{
    public bool skipClassIndentation = true; // used for C like stuff that has to hoist stuff out of class
    protected readonly EnumBuilder enumBuilder;
    private readonly RenderConfigVars renderConfig;
    protected readonly NameMangler mangler;
    protected readonly OutputFile file;
    protected readonly EventHandlerBuilder eventHandlerBuilder;
    protected readonly PseudoStateHandlerBuilder pseudoStateHandlerBuilder;

    protected StateMachine? _sm;
    protected StateMachine Sm => _sm.ThrowIfNull("Must be set before use");

    public AlgoBalanced1(NameMangler mangler, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, EnumBuilder enumBuilder, RenderConfigVars renderConfig, EventHandlerBuilder eventHandlerBuilder, CodeStyleSettings styler)
    {
        this.mangler = mangler;
        this.file = new OutputFile(styler, new StringBuilder());
        this.pseudoStateHandlerBuilder = pseudoStateHandlerBuilder;
        this.enumBuilder = enumBuilder;
        this.renderConfig = renderConfig;
        this.eventHandlerBuilder = eventHandlerBuilder;
    }

    public string GenerateGil(StateMachine sm)
    {
        this._sm = sm;
        mangler.SetStateMachine(sm);
        eventHandlerBuilder.SetFile(this.file);
        file.AppendLine($"// Generated state machine");
        file.Append($"public class {mangler.SmStructName}");

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

    // this is a bit of a hack that helps create the proper indentation for the Crill to C99 step
    private void RunWithPossibleIndentation(Action action)
    {
        if (skipClassIndentation)
            file.DecreaseIndentLevel();

        action();

        if (skipClassIndentation)
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
            file.AppendLine($"private delegate void {mangler.SmFuncTypedef}({mangler.SmStructName} sm);");
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

            //OutputFuncStateIdToString();

            OutputTriggerHandlers();
        });
    }

    internal void OutputStructDefinition()
    {
        file.AppendLine($"// Used internally by state machine. Feel free to inspect, but don't modify.");
        file.AppendLine($"public {mangler.SmStateEnum} state_id;");

        file.AppendLine();
        file.AppendLine($"// Used internally by state machine. Don't modify.");
        file.AppendLine($"private {mangler.SmFuncTypedef}? ancestor_event_handler;");

        file.AppendLine();
        file.AppendLine($"// Used internally by state machine. Don't modify.");
        file.AppendLine($"private readonly {mangler.SmFuncTypedef}?[] current_event_handlers = new {mangler.SmFuncTypedef}[{mangler.SmEventEnumCount}];");

        file.AppendLine();
        file.AppendLine($"// Used internally by state machine. Don't modify.");
        file.AppendLine($"private {mangler.SmFuncTypedef}? current_state_exit_handler;");

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
        file.Append($"public {mangler.SmStructName}()");
        file.StartCodeBlock();
        file.FinishCodeBlock();
        file.AppendLine();
    }

    //internal void OutputFuncStateIdToString()
    //{
    //    file.Append($"{ConstMarker}char* {mangler.SmFuncToString}({ConstMarker}{mangler.SmStateEnum} id)");  // todolow share function prototype string generation
    //    file.StartCodeBlock();
    //    {
    //        file.Append("switch (id)");
    //        file.StartCodeBlock();
    //        {
    //            foreach (var state in Sm.GetNamedVerticesCopy())
    //            {
    //                file.AppendLine($"case {mangler.SmStateEnumValue(state)}: return \"{mangler.SmStateToString(state)}\";");
    //            }
    //            file.AppendLine("default: return \"?\";");
    //        }
    //        file.FinishCodeBlock(forceNewLine: true);
    //    }
    //    file.FinishCodeBlock(forceNewLine: true);
    //    file.AppendLine();
    //}

    internal void OutputFuncStart()
    {
        file.AppendLine("// Starts the state machine. Must be called before dispatching events. Not thread safe.");
        file.Append($"public void {mangler.SmFuncStart}()");
        file.StartCodeBlock();
        file.AppendLine("ROOT_enter(this);");

        var initialState = Sm.Children.OfType<InitialState>().Single();

        var getToInitialStateBehavior = new Behavior(Sm, initialState);

        var tempSmAccess = eventHandlerBuilder.smAccess;
        eventHandlerBuilder.smAccess = "this";
        eventHandlerBuilder.OutputTransitionCode(getToInitialStateBehavior, noAncestorHandlesEvent: true, checkForExiting: false);
        eventHandlerBuilder.smAccess = tempSmAccess;

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    string ConstMarker => ""; // todo_low - put in an attribute like [ro] that will end up as `const` for languages that support that

    internal void OutputExitUpToFunction()
    {
        file.AppendLine("// This function is used when StateSmith doesn't know what the active leaf state is at");
        file.AppendLine("// compile time due to sub states or when multiple states need to be exited.");

        file.Append($"private static void {mangler.SmFuncExitUpTo}({mangler.SmName} sm, {ConstMarker}{mangler.SmFuncTypedef} desired_state_exit_handler)");
        file.StartCodeBlock();

        file.Append($"while (sm.current_state_exit_handler != desired_state_exit_handler)");
        file.StartCodeBlock();
        {
            file.AppendLine("sm.current_state_exit_handler!(sm);");
        }
        file.FinishCodeBlock(forceNewLine: true);

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    internal void OutputFuncDispatchEvent()
    {
        file.AppendLine("// Dispatches an event to the state machine. Not thread safe.");
        file.Append($"public void {mangler.SmFuncDispatchEvent}({mangler.SmEventEnum} event_id)");
        file.StartCodeBlock();
        file.AppendLine($"{mangler.SmFuncTypedef}? behavior_func = this.current_event_handlers[(int)event_id];");
        file.AppendLine();
        file.Append("while (behavior_func != null)");
        {
            file.StartCodeBlock();
            file.AppendLine("this.ancestor_event_handler = null;");
            file.AppendLine("behavior_func(this);");
            file.AppendLine("behavior_func = this.ancestor_event_handler;");
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
