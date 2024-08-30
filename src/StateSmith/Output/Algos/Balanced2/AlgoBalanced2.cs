#nullable enable

using StateSmith.Common;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Output.Algos.Balanced2;

public class AlgoBalanced2 : AlgoBalanced1
{
    public AlgoBalanced2(NameMangler mangler, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, EnumBuilder enumBuilder, RenderConfigBaseVars renderConfig, EventHandlerBuilder2 eventHandlerBuilder, CodeStyleSettings styler, AlgoBalanced1Settings settings, IAlgoEventIdToString algoEventIdToString, IAlgoStateIdToString algoStateIdToString, StandardFileHeaderPrinter standardFileHeaderPrinter) :
        base(mangler, pseudoStateHandlerBuilder, enumBuilder, renderConfig, eventHandlerBuilder, styler, settings, algoEventIdToString, algoStateIdToString, standardFileHeaderPrinter)
    {

    }

    override protected string GetAlgoName()
    {
        return nameof(AlgorithmId.Balanced2);
    }

    override protected void OutputAlgoBalanced1StructPointers()
    {
        // do nothing
    }

    override protected void OutputExitUpToFunction()
    {
        file.AppendLine("// This function is used when StateSmith doesn't know what the active leaf state is at");
        file.AppendLine("// compile time due to sub states or when multiple states need to be exited.");

        string desired_state = mangler.MangleVarName("desired_state");

        file.Append($"private void {mangler.SmExitUpToFuncName}({ConstMarker}{mangler.SmStateEnumType} {desired_state})");
        file.StartCodeBlock();

        file.Append($"while (this.{mangler.SmStateIdVarName} != {desired_state})");
        file.StartCodeBlock();
        {
            file.Append($"switch (this.{mangler.SmStateIdVarName})");
            file.StartCodeBlock();
            {
                bool needsLineSpacer = false;
                foreach (var namedVertex in Sm.GetNamedVerticesCopy())
                {
                    if (needsLineSpacer)
                    {
                        file.AppendLine();
                    }
                    needsLineSpacer = true;
                    file.Append($"case {mangler.SmQualifiedStateEnumValue(namedVertex)}: ");
                    string exitFunctionName = mangler.SmTriggerHandlerFuncName(namedVertex, TriggerHelper.TRIGGER_EXIT);

                    //file.IncreaseIndentLevel();
                    file.AppendWithoutIndent($"this.{exitFunctionName}(); ");
                    file.AppendWithoutIndent("break;\n");
                    //file.DecreaseIndentLevel();
                }
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    override protected void OutputFuncDispatchEvent()
    {
        file.AppendLine("// Dispatches an event to the state machine. Not thread safe.");
        string eventIdParameterName = mangler.MangleVarName("event_id");

        file.Append($"public void {mangler.SmDispatchEventFuncName}({mangler.SmEventEnumType} {eventIdParameterName})");
        file.StartCodeBlock();
        {
            file.Append($"switch (this.{mangler.SmStateIdVarName})");
            file.StartCodeBlock();
            {
                bool needsLineSpacer = false;
                foreach (var namedVertex in Sm.GetNamedVerticesCopy())
                {
                    if (needsLineSpacer)
                    {
                        file.AppendLine();
                    }
                    needsLineSpacer = true;

                    file.AppendLine($"// STATE: {namedVertex.Name}");
                    file.AppendLine($"case {mangler.SmQualifiedStateEnumValue(namedVertex)}:");

                    file.IncreaseIndentLevel();
                    OutputStateEventHandlerSwitch(namedVertex, eventIdParameterName);
                    file.AppendLine("break;");
                    file.DecreaseIndentLevel();
                }
            }
            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }
        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    private void OutputStateEventHandlerSwitch(NamedVertex namedVertex, string eventIdParameterName)
    {
        var allEvents = Sm.GetEventSet();

        // get list of all events that are handled by this state
        HashSet<string> events = TriggerHelper.GetEvents(namedVertex);

        file.Append($"switch ({eventIdParameterName})");
        file.StartCodeBlock();
        {
            foreach (string evt in events)
            {
                OutputEventHandler(namedVertex, evt);
            }

            var otherEvents = allEvents.Except(events);

            if (otherEvents.Any())
            {
                file.AppendLine($"// Events not handled by this state:");

                foreach (string evt in otherEvents)
                {
                    var ancestor = namedVertex.FirstAncestorThatHandlesEvent(evt);
                    OutputEventHandler(ancestor, evt);
                }
            }
        }
        file.FinishCodeBlock(forceNewLine: true);
    }

    private void OutputEventHandler(NamedVertex? namedVertex, string evt)
    {
        file.Append($"case {mangler.SmEventEnumType}.{mangler.SmEventEnumValue(evt)}: ");
        //file.IncreaseIndentLevel();

        if (namedVertex != null)
        {
            string eventHandlerFuncName = mangler.SmTriggerHandlerFuncName(namedVertex, evt);
            file.AppendWithoutIndent($"this.{eventHandlerFuncName}(); ");
        }

        file.AppendWithoutIndent("break;\n");
        //file.DecreaseIndentLevel();
    }
}
