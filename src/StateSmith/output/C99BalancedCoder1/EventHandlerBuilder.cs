#nullable enable

using System;
using System.Linq;
using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.Input.antlr4;

namespace StateSmith.Output.C99BalancedCoder1
{
    public class EventHandlerBuilder
    {
        private readonly CodeGenContext ctx;
        private readonly StateMachine sm;
        private readonly CNameMangler mangler;
        private readonly OutputFile file;

        public EventHandlerBuilder(CodeGenContext ctx, OutputFile file)
        {
            this.ctx = ctx;
            sm = ctx.sm;
            mangler = ctx.mangler;
            this.file = file;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="triggerName"></param>
        public void OutputStateBehaviorsForTrigger(NamedVertex state, string triggerName)
        {
            bool noAncestorHandlesEvent = true;

            if (TriggerHelper.IsEvent(triggerName))
            {
                noAncestorHandlesEvent = OutputNextAncestorHandler(state, triggerName);
            }

            var behaviorsWithTrigger = TriggerHelper.GetBehaviorsWithTrigger(state, triggerName);
            foreach (var b in behaviorsWithTrigger)
            {
                if (b.HasTransition())
                {
                    OutputTransitionCode(b, noAncestorHandlesEvent);
                }
                else
                {
                    OutputNonTransitionCode(b, triggerName, noAncestorHandlesEvent);
                }

                file.RequestNewLineBeforeMoreCode();
            }
        }

        public void OutputTransitionCode(Behavior behavior, bool noAncestorHandlesEvent)
        {
            OutputTransitionCodeInner(behavior, noAncestorHandlesEvent);
        }

        private void OutputGuardStart(Behavior b)
        {
            if (b.HasGuardCode())
            {
                string expandedGuardCode = ExpandingVisitor.ParseAndExpandCode(ctx.expander, b.guardCode);
                file.Append($"if ({expandedGuardCode})");
            }
        }

        private void DescribeBehaviorWithUmlComment(Behavior b)
        {
            file.AppendLine($"// uml: {b.DescribeAsUml()}");
        }

        private void OutputTransitionCodeInner(Behavior behavior, bool noAncestorHandlesEvent)
        {
            if (behavior.TransitionTarget == null)
            {
                throw new InvalidOperationException("shouldn't happen");
            }

            Vertex source = behavior.OwningVertex;
            Vertex target = behavior.TransitionTarget;

            OutputStartOfBehaviorCode(behavior);
            file.StartCodeBlock();
            {
                var transitionPath = source.FindTransitionPathTo(target);

                file.Append($"// Step 1: Exit states until we reach `{Vertex.Describe(transitionPath.leastCommonAncestor)}` state (Least Common Ancestor for transition).");
                if (IsExitingRequired(source, target, transitionPath))
                {
                    file.FinishLine();
                    ExitUntilStateReached(source, (NamedVertex)transitionPath.leastCommonAncestor.ThrowIfNull());
                }
                else
                {
                    file.FinishLine(" Already at LCA, no exiting required.");
                }
                file.RequestNewLineBeforeMoreCode();

                file.AppendLine($"// Step 2: Transition action: `{behavior.GetSingleLineActionCode()}`.");
                OutputAnyActionCode(behavior, isForTransition: true);
                file.RequestNewLineBeforeMoreCode();

                file.AppendLine($"// Step 3: Enter/move towards transition target `{Vertex.Describe(target)}`.");
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
                    file.AppendLine("// Step 4: complete transition. Ends event dispatch. No other behaviors are checked.");
                    file.AppendLine($"self->state_id = {mangler.SmStateEnumValue(namedVertexTarget)};");
                    if (noAncestorHandlesEvent)
                    {
                        file.AppendLine("// No ancestor handles event. Can skip nulling `self->ancestor_event_handler`.");
                    }
                    else
                    {
                        file.AppendLine("self->ancestor_event_handler = NULL;");
                    }
                    file.AppendLine($"return;");
                }
            }
        }

        private void EnterTowardsTarget(TransitionPath transitionPath)
        {
            if (transitionPath.toEnter.Count == 0)
            {
                file.AppendLine($"// Already in target. No entering required.");
                return;
            }

            foreach (var stateToEnter in transitionPath.toEnter)
            {
                if (stateToEnter is NamedVertex namedVertexToEnter)
                {
                    var enterHandler = mangler.SmFuncTriggerHandler(namedVertexToEnter, TriggerHelper.TRIGGER_ENTER);
                    file.AppendLine($"{enterHandler}(self);");
                }
                else if (stateToEnter is PseudoStateVertex pv)
                {
                    file.AppendLine($"// {Vertex.Describe(pv)} is a pseudo state and cannot have an `enter` trigger.");
                }
                else
                {
                    throw new ArgumentException("un-supported type: " + stateToEnter.GetType());
                }
            }
            file.RequestNewLineBeforeMoreCode();
        }

        private void OutputStartOfBehaviorCode(Behavior behavior)
        {
            file.AppendLine($"// {Vertex.Describe(behavior.OwningVertex)} behavior");
            DescribeBehaviorWithUmlComment(behavior);
            OutputGuardStart(behavior);
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

                var expandedAction = ExpandingVisitor.ParseAndExpandCode(ctx.expander, behavior.actionCode);
                file.AppendLines($"{expandedAction}");
                file.RequestNewLineBeforeMoreCode();
            }
        }

        private void OutputTransitionsForPseudoState(Behavior b, PseudoStateVertex pseudoState, bool noAncestorHandlesEvent)
        {
            string? transitionFunction = ctx.pseudoStateHandlerBuilder.MaybeGetFunctionName(pseudoState);

            if (transitionFunction != null)
            {
                file.AppendLine($"// Finish transition by calling pseudo state transition function.");
                file.AppendLine(transitionFunction + "(self);");
                file.AppendLine($"return; // event processing immediately stops when a transition finishes. No other behaviors for this state are checked.");
            }
            else
            {
                RenderPseudoStateTransitionsInner(pseudoState, noAncestorHandlesEvent);
            }
        }

        public void RenderPseudoStateTransitionFunctionInner(PseudoStateVertex pseudoState)
        {
            var functionName = ctx.pseudoStateHandlerBuilder.GetFunctionName(pseudoState); // just throws if not found

            const bool NoAncestorHandlesEvent = false; // assume ancestor might handle event because the pseudo state transition code can be called from multiple states.
            RenderPseudoStateTransitionsInner(pseudoState, noAncestorHandlesEvent: NoAncestorHandlesEvent);
        }

        private void RenderPseudoStateTransitionsInner(PseudoStateVertex pseudoState, bool noAncestorHandlesEvent)
        {
            foreach (Behavior pseudoStateBehavior in pseudoState.Behaviors)
            {
                if (pseudoStateBehavior.HasTransition())
                {
                    OutputTransitionCodeInner(pseudoStateBehavior, noAncestorHandlesEvent);
                    file.RequestNewLineBeforeMoreCode();
                }
            }
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

        private void ExitUntilStateReached(Vertex source, NamedVertex leastCommonAncestor)
        {
            bool canUseDirectExit = CanUseDirectExit(ref source, leastCommonAncestor);

            if (canUseDirectExit)
            {
                NamedVertex leafActiveState = (NamedVertex)source;
                string sourceExitHandler = mangler.SmFuncTriggerHandler(leafActiveState, TriggerHelper.TRIGGER_EXIT);
                file.AppendLine($"{sourceExitHandler}(self);");
            }
            else
            {
                string ancestorExitHandler = mangler.SmFuncTriggerHandler(leastCommonAncestor, TriggerHelper.TRIGGER_EXIT);
                file.AppendLine($"{mangler.SmFuncExitUpTo}(self, {ancestorExitHandler});");
            }
        }

        private static bool CanUseDirectExit(ref Vertex source, NamedVertex leastCommonAncestor)
        {
            bool canUseDirectExit = false; // if true, requires while loop exit

            if (source is ExitPoint)
            {
                source = source.NonNullParent;  // an exit point is really a part of its parent state.
                canUseDirectExit = true;
            }

            if (source is NamedVertex && source.Children.Any() == false && leastCommonAncestor == source.Parent)
            {
                canUseDirectExit = true;
            }

            return canUseDirectExit;
        }

        private bool OutputNextAncestorHandler(NamedVertex state, string triggerName)
        {
            bool noAncestorHandlesEvent;
            NamedVertex? nextHandlingState = state.FirstAncestorThatHandlesEvent(triggerName);
            noAncestorHandlesEvent = (nextHandlingState == null);

            if (nextHandlingState == null)
            {
                file.AppendLine($"// No ancestor state handles `{triggerName}` event.");
            }
            else
            {
                file.AppendLine($"// Setup handler for next ancestor that listens to `{triggerName}` event.");
                file.Append("self->ancestor_event_handler = ");
                file.FinishLine($"{mangler.SmFuncTriggerHandler(nextHandlingState, triggerName)};");
            }

            file.RequestNewLineBeforeMoreCode();
            return noAncestorHandlesEvent;
        }

        private void OutputNonTransitionCode(Behavior b, in string triggerName, bool noAncestorHandlesEvent)
        {
            bool isConsumable = TriggerHelper.IsEvent(triggerName);
            bool hasConsumeEventVar = isConsumable && b.HasActionCode();

            OutputStartOfBehaviorCode(b);
            file.StartCodeBlock();
            {
                MaybeOutputConsumeEventVariable(b, triggerName, noAncestorHandlesEvent, hasConsumeEventVar);

                file.AppendLine($"// Step 1: execute action `{b.GetSingleLineActionCode()}`");
                OutputAnyActionCode(b, isForTransition: false);

                MaybeOutputConsumeEventCode(noAncestorHandlesEvent: noAncestorHandlesEvent, isConsumable: isConsumable, hasConsumeEventVar: hasConsumeEventVar, triggerName: triggerName);
            }
            OutputEndOfBehaviorCode(b);
        }

        private void OutputEndOfBehaviorCode(Behavior b)
        {
            file.FinishCodeBlock($" // end of behavior for {Vertex.Describe(b.OwningVertex)}");
        }

        private void MaybeOutputConsumeEventCode(bool noAncestorHandlesEvent, bool isConsumable, bool hasConsumeEventVar, string triggerName)
        {
            if (!isConsumable)
            {
                return;
            }

            file.AppendLine($"// Step 2: determine if ancestor gets to handle event next.");

            if (hasConsumeEventVar)
            {
                if (noAncestorHandlesEvent)
                {
                    file.AppendLine("// No ancestor handles event. Ignore `consume_event` flag.");
                }
                else
                {
                    file.Append("if (consume_event)");
                    file.StartCodeBlock();
                    {
                        file.AppendLine("self->ancestor_event_handler = NULL;  // consume event");
                    }
                    file.FinishCodeBlock();
                }
            }
            else
            {

                if (TriggerHelper.IsDoEvent(triggerName))
                {
                    file.AppendLine("// Don't consume special `do` event.");
                }
                else
                {
                    if (noAncestorHandlesEvent)
                    {
                        file.AppendLine("// No ancestor handles event. Can skip nulling `self->ancestor_event_handler`.");
                    }
                    else
                    {
                        file.AppendLine("self->ancestor_event_handler = NULL;  // consume event");
                    }
                }
            }
        }

        private void MaybeOutputConsumeEventVariable(Behavior behavior, string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
        {
            if (!behavior.HasActionCode())
            {
                return;
            }

            if (!hasConsumeEventVar)
            {
                return;
            }

            if (noAncestorHandlesEvent)
            {
                file.AppendLine("// note: no ancestor consumes this event, but we output `bool consume_event` anyway because a user's design might rely on it.");
            }

            file.Append("bool consume_event = ");
            if (TriggerHelper.IsDoEvent(triggerName))
            {
                file.FinishLine("false; // the `do` event is special in that it normally is not consumed.");
            }
            else
            {
                file.FinishLine("true; // events other than `do` are normally consumed by any event handler. Other event handlers in *this* state may still handle the event though.");
            }
            // file.AppendLine("(void)consume_event; // avoid un-used variable compiler warning. StateSmith cannot (yet) detect if behavior action code sets `consume_event`.");
            file.AppendLine();
        }
    }
}
