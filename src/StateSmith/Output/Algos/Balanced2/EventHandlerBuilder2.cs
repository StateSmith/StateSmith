#nullable enable

using StateSmith.Common;
using StateSmith.Input.Expansions;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.UserConfig;
using StateSmith.SmGraph;
using System.Collections.Generic;

namespace StateSmith.Output.Algos.Balanced2;

public class EventHandlerBuilder2 : EventHandlerBuilder
{
    /// <summary>
    /// Just to be extra safe. Related to ensuring no infinite loop in "exit up to" function.
    /// https://github.com/StateSmith/StateSmith/issues/391
    /// </summary>
    public HashSet<NamedVertex> verticesWithExitStateIdAdjustment = new();

    public EventHandlerBuilder2(IExpander expander, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, INameMangler mangler, UserExpansionScriptBases userExpansionScriptBases, AlgoBalanced1Settings algoBalanced1Settings) : base(expander, pseudoStateHandlerBuilder, mangler, userExpansionScriptBases, algoBalanced1Settings)
    {
    }

    override protected void OutputExitUpToCall(NamedVertex leastCommonAncestor)
    {
        File.AppendIndentedLine($"this.{mangler.SmExitUpToFuncName}({mangler.SmQualifiedStateEnumValue(leastCommonAncestor)});");
    }

    // todolow - rename to something generic
    override protected void OutputStateExitFunctionPointerAdjustments(NamedVertex state, NamedVertex parentState)
    {
        // do nothing
    }

    override public void OutputFuncStateExit(NamedVertex state)
    {
        if (state is StateMachine)
        {
            // state machine roots don't need an exit function for Balanced2
        }
        else
        {
            base.OutputFuncStateExit(state);
        }
    }

    override protected void OutputExitFunctionEndCode(NamedVertex state)
    {
        // ex: sm->state_id = LightSm_StateId_ROOT;

        var parent = (NamedVertex?)state.Parent;
        if (parent != null)
        {
            File.AppendIndentedLine($"this.{mangler.SmStateIdVarName} = {mangler.SmQualifiedStateEnumValue(parent)};");
            verticesWithExitStateIdAdjustment.Add(state);
        }
    }

    override protected void OutputFuncStateEnter_PreBehaviors(NamedVertex state)
    {
        // NOTE! We need to track state as we enter it, so we can exit up to it later.
        // Some specified behaviors allow you to pass through a state (enter and exit immediately) towards another state.
        // This requires us to track the state we are entering.
        File.AppendIndentedLine($"this.{mangler.SmStateIdVarName} = {mangler.SmQualifiedStateEnumValue(state)};");
        File.RequestNewLineBeforeMoreCode();
    }

    override protected void PreActionCode(Behavior behavior, string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
    {
        if (!TriggerHelper.IsEvent(triggerName))
        {
            return;
        }

        // if transition behavior, no need to consume event. This method isn't called for transition behaviors though.

        // if no ancestor handles event, no need to consume event
        if (noAncestorHandlesEvent)
        {
            //File.AppendLine($"// No ancestor handles event {triggerName}. No need for `{consumeEventVarName}`.");
            return;
        }

        if (TriggerHelper.IsDoEvent(triggerName))
        {
            File.AppendIndentedLine($"// `do` events are not normally consumed.");
            return;
        }
        else
        {
            File.AppendIndentedLine($"// Consume event `{triggerName}`.");
            File.AppendIndentedLine($"{consumeEventVarName} = true;");
        }
    }

    override protected void PostActionCode(Behavior behavior, string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
    {
        
    }

    override protected void OutputStateEventHandlerFunctionTop(NamedVertex namedVertex, string eventName)
    {
        NamedVertex? nextHandlingState = namedVertex.FirstAncestorThatHandlesEvent(eventName);
        bool noAncestorHandlesEvent = nextHandlingState == null;

        bool hasConsumeEventVar = ActionCodeUsesConsumeEventVar(namedVertex, eventName) || !noAncestorHandlesEvent;

        if (hasConsumeEventVar)
        {
            if (noAncestorHandlesEvent)
            {
                File.AppendIndentedLine($"// note: no ancestor consumes this event, but we output `bool {consumeEventVarName}` anyway because a user's design might rely on it.");
            }

            File.AppendIndentedLine($"bool {consumeEventVarName} = false;");
            File.AppendIndentedLine();
        }
        else
        {
            //File.AppendLine($"// No need to output {consumeEventVarName}.");
        }
    }

    override protected void OutputStateEventHandlerFunctionBottom(NamedVertex namedVertex, string eventName)
    {
        NamedVertex? nextHandlingState = namedVertex.FirstAncestorThatHandlesEvent(eventName);

        if (nextHandlingState == null)
        {
            File.AppendIndentedLine($"// No ancestor handles this event.");
        }
        else
        {
            File.AppendIndentedLine($"// Check if event has been consumed before calling ancestor handler.");
            File.AppendIndented($"if (!{consumeEventVarName})");
            File.StartCodeBlock();
            {
                File.AppendIndentedLine($"this.{mangler.SmTriggerHandlerFuncName(nextHandlingState, eventName)}();");
            }
            File.FinishCodeBlock();
        }
    }

    override protected void OutputCompleteTransition(bool noAncestorHandlesEvent, NamedVertex namedVertexTarget)
    {
        File.AppendIndentedLine($"return;");
    }

    override protected void OutputNextAncestorHandler(NamedVertex? nextAncestorHandlingState, string triggerName)
    {
        // do nothing
    }
}
