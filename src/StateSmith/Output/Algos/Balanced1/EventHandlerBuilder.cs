#nullable enable

using System;
using System.Linq;
using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.Input.Antlr4;
using StateSmith.Input.Expansions;
using StateSmith.Output.Gil;
using System.Collections.Generic;
using StateSmith.Output.UserConfig;
using StateSmith.SmGraph.Validation;

namespace StateSmith.Output.Algos.Balanced1;

public class EventHandlerBuilder
{
    public string smAccess = "this";

    public const string consumeEventVarName = "consume_event";
    protected readonly INameMangler mangler;
    protected readonly PseudoStateHandlerBuilder pseudoStateHandlerBuilder;
    protected readonly WrappingExpander wrappingExpander;
    protected readonly UserExpansionScriptBases userExpansionScriptBases;
    protected readonly AlgoBalanced1Settings settings;

    protected OutputFile? _file;

    protected OutputFile File => _file.ThrowIfNull("You forgot to set file before using.");

    public EventHandlerBuilder(IExpander expander, PseudoStateHandlerBuilder pseudoStateHandlerBuilder, INameMangler mangler, UserExpansionScriptBases userExpansionScriptBases, AlgoBalanced1Settings settings)
    {
        this.pseudoStateHandlerBuilder = pseudoStateHandlerBuilder;
        this.mangler = mangler;
        this.wrappingExpander = new(expander, userExpansionScriptBases);
        this.userExpansionScriptBases = userExpansionScriptBases;
        this.settings = settings;
    }

    public void SetFile(OutputFile file)
    {
        _file = file;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namedVertex"></param>
    /// <param name="triggerName"></param>
    public void OutputStateBehaviorsForTrigger(NamedVertex namedVertex, string triggerName)
    {
        userExpansionScriptBases.UpdateNamedVertex(namedVertex);
        userExpansionScriptBases.UpdateCurrentTrigger(triggerName);

        bool noAncestorHandlesEvent = true;

        if (TriggerHelper.IsEvent(triggerName))
        {
            NamedVertex? nextAncestorHandlingState = namedVertex.FirstAncestorThatHandlesEvent(triggerName);
            noAncestorHandlesEvent = nextAncestorHandlingState == null;
            OutputNextAncestorHandler(nextAncestorHandlingState, triggerName);
        }

        var behaviorsWithTrigger = TriggerHelper.GetBehaviorsWithTrigger(namedVertex, triggerName);

        OutputReachableStateBehaviors(triggerName, noAncestorHandlesEvent, behaviorsWithTrigger);

        userExpansionScriptBases.UpdateNamedVertex(null);
        userExpansionScriptBases.UpdateCurrentTrigger(null);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/394
    /// </summary>
    /// <param name="triggerName">If null, only transitions are checked</param>
    /// <param name="noAncestorHandlesEvent"></param>
    /// <param name="behaviorsWithTrigger"></param>
    private void OutputReachableStateBehaviors(string? triggerName, bool noAncestorHandlesEvent, IEnumerable<Behavior> behaviorsWithTrigger)
    {
        List<Behavior>? unreachableBehaviors = null;
        foreach (var b in behaviorsWithTrigger)
        {
            if (unreachableBehaviors != null)
            {
                File.AppendIndentedLine($"// unreachable behavior: `{b.DescribeAsUml(singleLineFormat: true)}` due to unconditional transition above");
            }
            else
            {
                if (b.HasTransition())
                {
                    OutputTransitionCode(b, noAncestorHandlesEvent);
                    if (!b.HasGuardCode())
                    {
                        unreachableBehaviors = new();
                    }
                }
                else if (triggerName != null)
                {
                    OutputNonTransitionCode(b, triggerName, noAncestorHandlesEvent);
                }
            }

            File.RequestNewLineBeforeMoreCode();
        }
    }

    private void OutputGuardStart(Behavior b)
    {
        if (b.HasGuardCode())
        {
            string expandedGuardCode = wrappingExpander.ExpandWrapGuardCode(b);
            File.AppendLines($"if ({expandedGuardCode})");
        }
        else if (settings.useIfTrueIfNoGuard)
        {
            File.AppendIndented("if (true)");
        }
    }

    private void DescribeBehaviorWithUmlComment(Behavior b)
    {
        string uml = b.DescribeAsUml();
        File.AppendIndentedLine($"// uml: {uml}");
    }

    public void OutputTransitionCode(Behavior behavior, bool noAncestorHandlesEvent, bool checkForExiting = true)
    {
        if (behavior.TransitionTarget == null)
        {
            throw new InvalidOperationException("shouldn't happen");
        }

        Vertex source = behavior.OwningVertex;
        Vertex target = behavior.TransitionTarget;

        OutputStartOfBehaviorCode(behavior);
        File.StartCodeBlock();
        {
            var transitionPath = source.FindTransitionPathTo(target);

            File.AppendIndented($"// Step 1: Exit states until we reach `{Vertex.Describe(transitionPath.leastCommonAncestor)}` state (Least Common Ancestor for transition).");
            if (checkForExiting && IsExitingRequired(source, target, transitionPath))
            {
                File.FinishLine();
                ExitUntilLcaReached(source, transitionPath);
            }
            else
            {
                File.FinishLine(" Already at LCA, no exiting required.");
            }
            File.RequestNewLineBeforeMoreCode();

            File.AppendIndentedLine($"// Step 2: Transition action: `{behavior.GetSingleLineActionCode()}`.");
            OutputAnyActionCode(behavior, isForTransition: true);
            File.RequestNewLineBeforeMoreCode();

            File.AppendIndentedLine($"// Step 3: Enter/move towards transition target `{Vertex.Describe(target)}`.");
            EnterTowardsTarget(transitionPath);

            FinishTransitionOrContinuePseudo(behavior, target, noAncestorHandlesEvent);
        }
        OutputEndOfBehaviorCode(behavior);
    }

    private void FinishTransitionOrContinuePseudo(Behavior behavior, Vertex target, bool noAncestorHandlesEvent)
    {
        if (target is PseudoStateVertex pseudoStateVertex)
        {
            OutputTransitionsForPseudoState(behavior, pseudoStateVertex, noAncestorHandlesEvent);
        }
        else if (target is NamedVertex namedVertexTarget)
        {
            InitialState? initialState = namedVertexTarget.Children.OfType<InitialState>().FirstOrDefault();

            if (initialState != null)
            {
                OutputTransitionsForPseudoState(behavior, initialState, noAncestorHandlesEvent);
            }
            else
            {
                // no initial state, this is the final state.
                File.AppendIndentedLine("// Step 4: complete transition. Ends event dispatch. No other behaviors are checked.");
                OutputCompleteTransition(noAncestorHandlesEvent, namedVertexTarget);
            }
        }
    }

    virtual protected void OutputCompleteTransition(bool noAncestorHandlesEvent, NamedVertex namedVertexTarget)
    {
        File.AppendIndentedLine($"{smAccess}.{mangler.SmStateIdVarName} = {mangler.SmQualifiedStateEnumValue(namedVertexTarget)};");
        NullAncestorEventHandlerVar(noAncestorHandlesEvent);
        File.AppendIndentedLine($"return;");
    }

    private void NullAncestorEventHandlerVar(bool noAncestorHandlesEvent)
    {
        if (noAncestorHandlesEvent)
        {
            File.AppendIndentedLine($"// No ancestor handles event. Can skip nulling `{mangler.SmAncestorEventHandlerVarName}`.");
        }
        else
        {
            File.AppendIndentedLine($"{smAccess}.{mangler.SmAncestorEventHandlerVarName} = null;");
        }
    }

    private void EnterTowardsTarget(TransitionPath transitionPath)
    {
        if (transitionPath.toEnter.Count == 0)
        {
            File.AppendIndentedLine($"// Already in target. No entering required.");
            return;
        }

        foreach (var stateToEnter in transitionPath.toEnter)
        {
            if (stateToEnter is NamedVertex namedVertexToEnter)
            {
                var enterHandler = mangler.SmTriggerHandlerFuncName(namedVertexToEnter, TriggerHelper.TRIGGER_ENTER);
                File.AppendIndentedLine($"this.{enterHandler}();");
            }
            else if (stateToEnter is PseudoStateVertex pv)
            {
                File.AppendIndentedLine($"// {Vertex.Describe(pv)} is a pseudo state and cannot have an `enter` trigger.");
            }
            else
            {
                throw new ArgumentException("un-supported type: " + stateToEnter.GetType());
            }
        }
        File.RequestNewLineBeforeMoreCode();
    }

    private void OutputStartOfBehaviorCode(Behavior behavior)
    {
        File.AppendIndentedLine($"// {Vertex.Describe(behavior.OwningVertex)} behavior");
        DescribeBehaviorWithUmlComment(behavior);
        OutputGuardStart(behavior);
    }

    protected bool ActionCodeUsesConsumeEventVar(Behavior behavior)
    {
        if (behavior.HasActionCode())
        {
            var expandedAction = wrappingExpander.ExpandActionCode(behavior);
            var inspector = new ActionCodeInspector();
            inspector.Parse(expandedAction);
            if (inspector.identifiersUsed.Contains(consumeEventVarName))
                return true;
        }

        return false;
    }

    protected bool ActionCodeUsesConsumeEventVar(NamedVertex namedVertex, string eventName)
    {
        var behaviorsWithTrigger = TriggerHelper.GetBehaviorsWithTrigger(namedVertex, eventName);

        foreach (var behavior in behaviorsWithTrigger)
        {
            if (ActionCodeUsesConsumeEventVar(behavior))
                return true;
        }

        return false;
    }

    private void OutputAnyActionCode(Behavior behavior, bool isForTransition)
    {
        if (behavior.HasActionCode())
        {
            if (isForTransition)
            {
                if (behavior.OwningVertex is NamedVertex)
                {
                    // file.AppendLine("// Note: no `consume_event` variable possible here because of state transition. The event must be consumed.");
                }
            }

            var expandedAction = wrappingExpander.ExpandWrapActionCode(behavior);
            File.AppendLines(expandedAction);
            File.RequestNewLineBeforeMoreCode();
        }
    }

    private void OutputTransitionsForPseudoState(Behavior b, PseudoStateVertex pseudoState, bool noAncestorHandlesEvent)
    {
        string? transitionFunction = pseudoStateHandlerBuilder.MaybeGetFunctionName(pseudoState);

        if (transitionFunction != null)
        {
            File.AppendIndentedLine($"// Finish transition by calling pseudo state transition function.");
            File.AppendIndentedLine($"this.{transitionFunction}();");
            File.AppendIndentedLine($"return; // event processing immediately stops when a transition finishes. No other behaviors for this state are checked.");
        }
        else
        {
            RenderPseudoStateTransitionsInner(pseudoState, noAncestorHandlesEvent);
        }
    }

    public void RenderPseudoStateTransitionFunctionInner(PseudoStateVertex pseudoState)
    {
        pseudoStateHandlerBuilder.GetFunctionName(pseudoState); // just throws if not found

        const bool NoAncestorHandlesEvent = false; // assume ancestor might handle event because the pseudo state transition code can be called from multiple states.
        RenderPseudoStateTransitionsInner(pseudoState, noAncestorHandlesEvent: NoAncestorHandlesEvent);
    }

    private void RenderPseudoStateTransitionsInner(PseudoStateVertex pseudoState, bool noAncestorHandlesEvent)
    {
        string? noTriggerName = null;  // means that only transitions are checked
        OutputReachableStateBehaviors(triggerName: noTriggerName, noAncestorHandlesEvent, pseudoState.Behaviors);
    }

    private static bool IsExitingRequired(Vertex source, Vertex target, TransitionPath transitionPath)
    {
        if (source is ExitPoint && source.Parent == target)
        {
            // self transition. exit required
            return true;
        }

        // If vertex is a pseudo state, then we know active leaf state is the containing state.
        // If it is also the LCA the the transition, we know no exiting is required at this point.
        if (source is PseudoStateVertex)
        {
            source = source.Parent!;

            if (transitionPath.leastCommonAncestor == source)
            {
                return false;
            }
        }

        return true;
    }

    private void ExitUntilLcaReached(Vertex source, TransitionPath transitionPath)
    {
        bool canUseDirectExit = CanUseSingleDirectExit(ref source, transitionPath);

        if (canUseDirectExit)
        {
            NamedVertex leafActiveState = (NamedVertex)source;
            string sourceExitHandler = mangler.SmTriggerHandlerFuncName(leafActiveState, TriggerHelper.TRIGGER_EXIT);
            File.AppendIndentedLine($"this.{sourceExitHandler}();");
        }
        else
        {
            NamedVertex leastCommonAncestor = (NamedVertex)transitionPath.leastCommonAncestor.ThrowIfNull();
            OutputExitUpToCall(leastCommonAncestor);
        }
    }

    virtual protected void OutputExitUpToCall(NamedVertex leastCommonAncestor)
    {
        string ancestorExitHandler = mangler.SmTriggerHandlerFuncName(leastCommonAncestor, TriggerHelper.TRIGGER_EXIT);
        File.AppendIndentedLine($"this.{mangler.SmExitUpToFuncName}(this.{ancestorExitHandler});");
    }

    private static bool CanUseSingleDirectExit(ref Vertex source, TransitionPath transitionPath)
    {
        bool canUseDirectExit = false;

        // We can only be exiting one state to use direct exit.
        var statesToExitCount = transitionPath.toExit.Count;

        // If source doesn't have any children, we know that it is the active leaf state and we may be able
        // to use a direct exit.
        bool sourceIsLeafState = source.Children.Any() == false;

        if (source is ExitPoint)
        {
            // This code assumes that first node to exit in transition path is source exit point.
            // That may change in the future as pseudo states cannot be exited so we add an assertion below to catch any future problems.
            if (transitionPath.toExit.First() != source)
                throw new VertexValidationException(source, "expected transition path exit list to start with source vertex");

            statesToExitCount--; // an exit point isn't a state that can be exited so decrement count.
            source = source.NonNullParent;  // an exit point should be treated as parent state.
        }

        if (source is NamedVertex && sourceIsLeafState && statesToExitCount <= 1)
        {
            canUseDirectExit = true;
        }

        return canUseDirectExit;
    }

    virtual protected void OutputNextAncestorHandler(NamedVertex? nextAncestorHandlingState, string triggerName)
    {
        if (nextAncestorHandlingState == null)
        {
            File.AppendIndentedLine($"// No ancestor state handles `{triggerName}` event.");
        }
        else
        {
            File.AppendIndentedLine($"// Setup handler for next ancestor that listens to `{triggerName}` event.");
            File.AppendIndentedLine($"{smAccess}.{mangler.SmAncestorEventHandlerVarName} = this.{mangler.SmTriggerHandlerFuncName(nextAncestorHandlingState, triggerName)};");
        }

        File.RequestNewLineBeforeMoreCode();
    }

    private void OutputNonTransitionCode(Behavior b, in string triggerName, bool noAncestorHandlesEvent)
    {
        bool isEvent = TriggerHelper.IsEvent(triggerName);
        bool hasConsumeEventVar = isEvent && ActionCodeUsesConsumeEventVar(b);

        OutputStartOfBehaviorCode(b);
        File.StartCodeBlock();
        {
            PreActionCode(b, triggerName, noAncestorHandlesEvent, hasConsumeEventVar);

            File.AppendIndentedLine($"// Step 1: execute action `{b.GetSingleLineActionCode()}`");
            OutputAnyActionCode(b, isForTransition: false);

            PostActionCode(b, triggerName, noAncestorHandlesEvent, hasConsumeEventVar);
        }
        OutputEndOfBehaviorCode(b);
    }

    private void OutputEndOfBehaviorCode(Behavior b)
    {
        File.FinishCodeBlock($" // end of behavior for {Vertex.Describe(b.OwningVertex)}");
    }


    private void MaybeOutputConsumeTriggerCode(bool noAncestorHandlesEvent, bool isEvent, bool hasConsumeEventVar, string triggerName)
    {
        if (!isEvent)
        {
            return;
        }

        File.AppendIndentedLine($"// Step 2: determine if ancestor gets to handle event next.");

        if (hasConsumeEventVar)
        {
            if (noAncestorHandlesEvent)
            {
                File.AppendIndentedLine($"// No ancestor handles event. Ignore `{consumeEventVarName}` flag.");
            }
            else
            {
                File.AppendIndented($"if ({consumeEventVarName})");
                File.StartCodeBlock();
                {
                    File.AppendIndentedLine($"{smAccess}.{mangler.SmAncestorEventHandlerVarName} = null;  // consume event");
                }
                File.FinishCodeBlock();
            }
        }
        else
        {
            // user hasn't explicitly controlled the consume_event variable, so we may need to null the ancestor event handler.

            if (TriggerHelper.IsDoEvent(triggerName))
            {
                File.AppendIndentedLine("// Don't consume special `do` event.");
            }
            else
            {
                if (noAncestorHandlesEvent)
                {
                    File.AppendIndentedLine($"// No ancestor handles event. Can skip nulling `{mangler.SmAncestorEventHandlerVarName}`.");
                }
                else
                {
                    File.AppendIndentedLine($"{smAccess}.{mangler.SmAncestorEventHandlerVarName} = null;  // consume event");
                }
            }
        }
    }

    virtual protected void PreActionCode(Behavior behavior, string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
    {
        MaybeOutputBehaviorConsumeEventVariable(behavior, triggerName, noAncestorHandlesEvent, hasConsumeEventVar);
    }

    virtual protected void PostActionCode(Behavior behavior, string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
    {
        MaybeOutputConsumeTriggerCode(noAncestorHandlesEvent: noAncestorHandlesEvent, isEvent: TriggerHelper.IsEvent(triggerName), hasConsumeEventVar: hasConsumeEventVar, triggerName: triggerName);
    }

    private void MaybeOutputBehaviorConsumeEventVariable(Behavior behavior, string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
    {
        if (!behavior.HasActionCode())
        {
            return;
        }

        if (!hasConsumeEventVar)
        {
            return;
        }

        OutputConsumeEventVar(triggerName, noAncestorHandlesEvent);
    }

    protected void OutputConsumeEventVar(string triggerName, bool noAncestorHandlesEvent)
    {
        if (noAncestorHandlesEvent)
        {
            File.AppendIndentedLine($"// note: no ancestor consumes this event, but we output `bool {consumeEventVarName}` anyway because a user's design might rely on it.");
        }

        File.AppendIndented($"bool {consumeEventVarName} = ");
        if (TriggerHelper.IsDoEvent(triggerName))
        {
            File.FinishLine("false; // the `do` event is special in that it normally is not consumed.");
        }
        else
        {
            File.FinishLine("true; // events other than `do` are normally consumed by any event handler. Other event handlers in *this* state may still handle the event though.");
        }
        File.AppendIndentedLine();
    }

    public void OutputNamedStateHandlers(NamedVertex state)
    {
        File.AppendIndentedLine();
        File.AppendIndentedLine("////////////////////////////////////////////////////////////////////////////////");
        File.AppendIndentedLine($"// event handlers for state {mangler.SmStateName(state)}");
        File.AppendIndentedLine("////////////////////////////////////////////////////////////////////////////////");
        File.AppendIndentedLine();

        OutputFuncStateEnter(state);
        OutputFuncStateExit(state);

        string[] eventNames = GetEvents(state).ToArray();
        Array.Sort(eventNames);


        foreach (var eventName in eventNames)
        {
            OutputStateEventHandler(state, eventName);
        }

        userExpansionScriptBases.UpdateNamedVertex(state);
        pseudoStateHandlerBuilder.OutputFunctionsForParent(state, RenderPseudoStateTransitionFunctionInner);
        userExpansionScriptBases.UpdateNamedVertex(null);
    }

    private void OutputStateEventHandler(NamedVertex state, string eventName)
    {
        OutputTriggerHandlerSignature(state, eventName);
        File.StartCodeBlock();
        {
            userExpansionScriptBases.UpdateNamedVertex(state);
            {
                OutputStateEventHandlerFunctionTop(state, eventName);
                OutputStateBehaviorsForTrigger(state, eventName);
                OutputStateEventHandlerFunctionBottom(state, eventName);
            }
            userExpansionScriptBases.UpdateNamedVertex(null);
        }
        FinishAddressableFunction(forceNewLine: false);
        File.RequestNewLineBeforeMoreCode();
    }

    virtual protected void OutputStateEventHandlerFunctionTop(NamedVertex state, string eventName)
    {
        // do nothing
    }

    virtual protected void OutputStateEventHandlerFunctionBottom(NamedVertex state, string eventName)
    {
        // do nothing
    }

    public void OutputFuncStateEnter(NamedVertex state)
    {
        OutputTriggerHandlerSignature(state, TriggerHelper.TRIGGER_ENTER);

        File.StartCodeBlock();
        {
            OutputFuncStateEnter_PreBehaviors(state);
            OutputStateBehaviorsForTrigger(state, TriggerHelper.TRIGGER_ENTER);
        }
        File.FinishCodeBlock(forceNewLine: true);
        File.AppendIndentedLine();
    }

    virtual protected void OutputFuncStateEnter_PreBehaviors(NamedVertex state)
    {
        OutputStateEnterFunctionPointerAdjustments(state);
    }

    private void OutputStateEnterFunctionPointerAdjustments(NamedVertex state)
    {
        File.AppendIndentedLine($"// setup trigger/event handlers");
        string stateExitHandlerName = mangler.SmTriggerHandlerFuncName(state, TriggerHelper.TRIGGER_EXIT);
        File.AppendIndentedLine($"{smAccess}.{mangler.SmCurrentStateExitHandlerVarName} = this.{stateExitHandlerName};");

        string[] eventNames = GetEvents(state).ToArray();
        Array.Sort(eventNames);

        foreach (var eventName in eventNames)
        {
            string handlerName = mangler.SmTriggerHandlerFuncName(state, eventName);
            string eventEnumValueName = mangler.SmEventEnumValue(eventName);
            File.AppendIndentedLine($"{smAccess}.{mangler.SmCurrentEventHandlersVarName}[(int){mangler.SmEventEnumType}.{eventEnumValueName}] = this.{handlerName};");
        }

        File.RequestNewLineBeforeMoreCode();
    }

    public virtual void OutputFuncStateExit(NamedVertex state)
    {
        OutputTriggerHandlerSignature(state, TriggerHelper.TRIGGER_EXIT);

        File.StartCodeBlock();
        {
            OutputStateBehaviorsForTrigger(state, TriggerHelper.TRIGGER_EXIT);

            if (state.Parent == null)
            {
                File.AppendIndentedLine($"// State machine root is a special case. It cannot be exited. Mark as unused.");
                File.AppendIndentedLine(GilCreationHelper.MarkVarAsUnused("this"));
            }
            else
            {
                OutputStateExitFunctionPointerAdjustments(state, parentState: (NamedVertex)state.Parent);
            }

            OutputExitFunctionEndCode(state);
        }

        FinishAddressableFunction(forceNewLine: true);
        File.AppendIndentedLine();
    }

    virtual protected void OutputExitFunctionEndCode(NamedVertex state)
    {
        // do nothing. override in derived class.
    }

    virtual protected void OutputStateExitFunctionPointerAdjustments(NamedVertex state, NamedVertex parentState)
    {
        File.AppendIndentedLine($"// adjust function pointers for this state's exit");
        string parentExitHandler = mangler.SmTriggerHandlerFuncName(parentState, TriggerHelper.TRIGGER_EXIT);
        File.AppendIndentedLine($"{smAccess}.{mangler.SmCurrentStateExitHandlerVarName} = this.{parentExitHandler};");

        string[] eventNames = GetEvents(state).ToArray();
        Array.Sort(eventNames);

        foreach (var eventName in eventNames)
        {
            string eventEnumValueIndex = $"(int){mangler.SmEventEnumType}.{mangler.SmEventEnumValue(eventName)}";
            var ancestor = state.FirstAncestorThatHandlesEvent(eventName);
            if (ancestor != null)
            {
                string handlerName = mangler.SmTriggerHandlerFuncName(ancestor, eventName);
                File.AppendIndentedLine($"{smAccess}.{mangler.SmCurrentEventHandlersVarName}[{eventEnumValueIndex}] = this.{handlerName};  // the next ancestor that handles this event is {mangler.SmStateName(ancestor)}");
            }
            else
            {
                File.AppendIndentedLine($"{smAccess}.{mangler.SmCurrentEventHandlersVarName}[{eventEnumValueIndex}] = null;  // no ancestor listens to this event");
            }
        }
    }

    private void FinishAddressableFunction(bool forceNewLine)
    {
        var finishStr = "";

        File.FinishCodeBlock(finishStr, forceNewLine: forceNewLine);
    }

    public void OutputTriggerHandlerSignature(NamedVertex state, string eventName)
    {
        // enter functions don't need to be static delegates because we don't take their address
        string funcName = mangler.SmTriggerHandlerFuncName(state, eventName);
        File.AppendIndented($"private void {funcName}()");
    }

    /// <summary>
    /// These do NOT include entry and exit triggers
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static HashSet<string> GetEvents(NamedVertex state)
    {
        return TriggerHelper.GetEvents(state);
    }
}
