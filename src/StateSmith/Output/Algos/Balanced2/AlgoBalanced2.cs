#nullable enable

using StateSmith.Common;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Gil;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Output.Algos.Balanced2;

public class AlgoBalanced2 : AlgoBalanced1
{
    private const string FirstAncestorHandlerComment = "First ancestor handler for this event";

    /// <summary>
    /// Just to be extra safe. Related to ensuring no infinite loop in "exit up to" function.
    /// https://github.com/StateSmith/StateSmith/issues/391
    /// </summary>
    public HashSet<NamedVertex> verticesThatNeedExitStateIdAdjustment = new();

    EventHandlerBuilder2 eventHandlerBuilder2;

    public AlgoBalanced2(NameMangler mangler, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, EnumBuilder enumBuilder, RenderConfigBaseVars renderConfig, EventHandlerBuilder2 eventHandlerBuilder2, CodeStyleSettings styler, AlgoBalanced1Settings settings, IAlgoEventIdToString algoEventIdToString, IAlgoStateIdToString algoStateIdToString, StandardFileHeaderPrinter standardFileHeaderPrinter) :
        base(mangler, pseudoStateHandlerBuilder, enumBuilder, renderConfig, eventHandlerBuilder2, styler, settings, algoEventIdToString, algoStateIdToString, standardFileHeaderPrinter)
    {
        this.eventHandlerBuilder2 = eventHandlerBuilder2;
    }

    override protected void FinalCheck()
    {
        // https://github.com/StateSmith/StateSmith/issues/391
        var unhandledVertices = verticesThatNeedExitStateIdAdjustment.Except(eventHandlerBuilder2.verticesWithExitStateIdAdjustment);

        if (unhandledVertices.Any())
        {
            throw new VertexValidationException(unhandledVertices.First(), "Found vertex what wasn't handled properly for exiting.");
        }
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
                foreach (var namedVertex in Sm.GetNamedVerticesCopy())
                {

                    if (namedVertex is StateMachine)
                    {
                        // do nothing. let default case handle this.
                    }
                    else
                    {
                        file.Append($"case {mangler.SmQualifiedStateEnumValue(namedVertex)}: ");
                        string exitFunctionName = mangler.SmTriggerHandlerFuncName(namedVertex, TriggerHelper.TRIGGER_EXIT);
                        file.AppendWithoutIndent($"this.{exitFunctionName}(); ");
                        file.AppendWithoutIndent("break;\n");
                        file.AppendLine();
                        verticesThatNeedExitStateIdAdjustment.Add(namedVertex);
                    }
                }

                file.AppendLine($"default: return;  // Just to be safe. Prevents infinite loop if state ID memory is somehow corrupted.");
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
        file.FinishCodeBlock(forceNewLine: true);

        file.FinishCodeBlock(forceNewLine: true);
        file.AppendLine();
    }

    override protected void OutputFuncDispatchEvent()
    {
        OutputFuncDispatchEventStart(out string eventIdParameterName);

        file.StartCodeBlock();
        {
            // optimization for single event
            // https://github.com/StateSmith/StateSmith/issues/388
            if (Sm.GetEventSet().Count == 1)
            {
                file.AppendLine("// This state machine design only has a single event type so we can safely assume");
                file.AppendLine($"// that the dispatched event is `{Sm.GetEventSet().Single()}` without checking the `{eventIdParameterName}` parameter.");
                file.AppendLine(GilCreationHelper.MarkVarAsUnused(eventIdParameterName) + " // This line prevents an 'unused variable' compiler warning"); // Note! transpilers that don't need this will skip this line and trailing comments/trivia.
                file.AppendLine();
            }

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
                    OutputStateEventHandlerCode(namedVertex, eventIdParameterName);
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

    private void OutputStateEventHandlerCode(NamedVertex namedVertex, string eventIdParameterName)
    {
        var allEvents = Sm.GetEventSet();

        // get list of all events that are handled by this state
        HashSet<string> stateEvents = TriggerHelper.GetEvents(namedVertex);

        // optimization for when a switch is not required (single event)
        if (allEvents.Count == 1)
        {
            OutputStateSingleEventHandlerCode(namedVertex, allEvents, stateEvents);
        }
        else
        {
            file.Append($"switch ({eventIdParameterName})");
            file.StartCodeBlock();
            {
                foreach (string evt in stateEvents)
                {
                    MaybeOutputEventHandler(namedVertex, evt);
                }

                var otherEvents = allEvents.Except(stateEvents);

                if (otherEvents.Any())
                {
                    file.AppendLine($"// Events not handled by this state:");

                    foreach (string evt in otherEvents)
                    {
                        NamedVertex? ancestor = namedVertex.FirstAncestorThatHandlesEvent(evt);
                        var comment = (ancestor == null) ? string.Empty : FirstAncestorHandlerComment;
                        MaybeOutputEventHandler(ancestor, evt, comment: comment);
                    }
                }
            }
            file.FinishCodeBlock(forceNewLine: true);
        }
    }

    /// <summary>
    /// optimization for when a switch is not required (single event in entire state machine).
    /// https://github.com/StateSmith/StateSmith/issues/388
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void OutputStateSingleEventHandlerCode(NamedVertex namedVertex, IReadOnlySet<string> allEvents, HashSet<string> stateEvents)
    {
        string onlyEvent = allEvents.Single();
        if (stateEvents.Count > 1)
        {
            throw new Exception($"Unexpected number of events: {stateEvents.Count}. Only expected: `{onlyEvent}`.");
        }

        file.Append();
        bool outputHandler = false;
        if (stateEvents.Any())
        {
            outputHandler = true;
            OutputEventHandlerCall(namedVertex, onlyEvent);
        }
        else
        {
            var ancestor = namedVertex.FirstAncestorThatHandlesEvent(onlyEvent);
            if (ancestor != null)
            {
                outputHandler = true;
                OutputEventHandlerCall(ancestor, onlyEvent);
                AddFirstAncestorHandlerComment();
            }
        }

        if (!outputHandler)
        {
            file.AppendWithoutIndent($"// state and ancestors have no handler for `{onlyEvent}` event.");
        }

        file.AppendWithoutIndent("\n");
    }

    private void MaybeOutputEventHandler(NamedVertex? namedVertex, string evt, string comment = "")
    {
        file.Append($"case {mangler.SmEventEnumType}.{mangler.SmEventEnumValue(evt)}: ");
        //file.IncreaseIndentLevel();

        if (namedVertex != null)
        {
            OutputEventHandlerCall(namedVertex, evt);
        }

        file.AppendWithoutIndent("break;");

        AddCommentLine(comment);

        //file.DecreaseIndentLevel();
    }

    private void AddFirstAncestorHandlerComment()
    {
        AddCommentLine(FirstAncestorHandlerComment);
    }

    private void AddCommentLine(string comment)
    {
        if (comment.Length > 0)
        {
            file.AppendWithoutIndent(" // " + comment);
        }
        file.AppendWithoutIndent("\n");
    }

    private void OutputEventHandlerCall(NamedVertex namedVertex, string evt)
    {
        string eventHandlerFuncName = mangler.SmTriggerHandlerFuncName(namedVertex, evt);
        file.AppendWithoutIndent($"this.{eventHandlerFuncName}(); ");
    }

    protected override void OutputEventHandlerDelegate()
    {
        // do nothing
    }
}
