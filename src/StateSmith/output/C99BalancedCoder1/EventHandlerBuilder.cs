#nullable enable

using System;
using System.Linq;
using StateSmith.Common;
using StateSmith.Compiling;
using StateSmith.compiler;
using StateSmith.Input.antlr4;

namespace StateSmith.output.C99BalancedCoder1
{
    public class EventHandlerBuilder
    {
        private readonly CodeGenContext ctx;
        private readonly Statemachine sm;
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
                    OutputTransitionCode(b);
                }
                else
                {
                    OutputNonTransitionCode(b, triggerName, noAncestorHandlesEvent);
                }

                file.RequestNewLineBeforeMoreCode();
            }
        }

        public void OutputTransitionCode(Behavior behavior)
        {
            OutputTransitionCodeInner(behavior);
        }

        private string GetTransitionGuardCondition(Behavior b)
        {
            string expandedGuardCode;

            if (b.HasGuardCode())
            {
                expandedGuardCode = ExpandingVisitor.ParseAndExpandCode(ctx.expander, b.guardCode);
            }
            else
            {
                expandedGuardCode = "true";
            }

            return expandedGuardCode;
        }

        private void DescribeBehaviorWithUmlComment(Behavior b)
        {
            file.AppendLine($"// uml: {b.DescribeAsUml()}");
        }

        private void OutputTransitionCodeInner(Behavior behavior)
        {
            if (behavior._transitionTarget == null)
            {
                throw new InvalidOperationException("shouldn't happen");
            }

            Vertex source = behavior.OwningVertex;
            Vertex target = behavior._transitionTarget;

            OutputStartOfBehaviorCode(behavior);
            file.StartCodeBlock();
            {
                if (behavior.OwningVertex is NamedVertex)
                {
                    file.AppendLine("// Note: no `consume_event` variable possible here because of state transition. The event must be consumed.");
                }

                var transitionPath = source.FindTransitionPathTo(target);

                if (IsExitingRequired(source))
                {
                    ExitUntilStateReached(source, (NamedVertex)transitionPath.leastCommonAncestor);
                }

                OutputAnyActionCode(behavior);

                file.AppendLine();
                EnterTowardsTarget(transitionPath);

                FinishTransitionOrContinuePseudo(behavior, target);
            }
            OutputEndOfBehaviorCode(behavior);
        }

        private void FinishTransitionOrContinuePseudo(Behavior behavior, Vertex target)
        {
            if (target is PseudoStateVertex pseudoStateVertex)
            {
                OutputTransitionsForPseudoState(behavior, pseudoStateVertex);
            }
            else if (target is NamedVertex namedVertexTarget)
            {
                InitialState? initialState = namedVertexTarget.Children.OfType<InitialState>().FirstOrDefault();

                if (initialState != null)
                {
                    OutputTransitionsForPseudoState(behavior, initialState);
                }
                else
                {
                    // no initial state, this is the final state.
                    file.AppendLine("// update state_id");
                    file.AppendLine($"self->state_id = {mangler.SmStateEnumValue(namedVertexTarget)};");
                    file.AppendLine("self->ancestor_event_handler = NULL;"); // todolow - only do if owning state actually needs it.
                    file.AppendLine($"return; // event processing immediately stops when a transition finishes. No other behaviors for this state are checked.");
                }
            }
        }

        private void EnterTowardsTarget(TransitionPath transitionPath)
        {
            file.AppendLine("// Enter towards target");
            foreach (var stateToEnter in transitionPath.toEnter)
            {
                if (stateToEnter is NamedVertex namedVertexToEnter)
                {
                    var enterHandler = mangler.SmFuncTriggerHandler(namedVertexToEnter, TriggerHelper.TRIGGER_ENTER);
                    file.AppendLine($"{enterHandler}(self);");
                }
                else if (stateToEnter is PseudoStateVertex pv)
                {
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
            file.Append($"if ({GetTransitionGuardCondition(behavior)})");
        }

        private void OutputAnyActionCode(Behavior behavior)
        {
            if (behavior.HasActionCode())
            {
                var expandedAction = ExpandingVisitor.ParseAndExpandCode(ctx.expander, behavior.actionCode);
                file.AppendLines($"{expandedAction}");
            }
        }

        private void OutputTransitionsForPseudoState(Behavior b, PseudoStateVertex pseudoState)
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
                RenderPseudoStateTransitionsInner(pseudoState);
            }
        }

        public void RenderPseudoStateTransitionFunctionInner(PseudoStateVertex pseudoState)
        {
            ctx.pseudoStateHandlerBuilder.GetFunctionName(pseudoState); // just throws if not found
            RenderPseudoStateTransitionsInner(pseudoState);
        }

        private void RenderPseudoStateTransitionsInner(PseudoStateVertex pseudoState)
        {
            foreach (Behavior pseudoStateBehavior in pseudoState.Behaviors)
            {
                if (pseudoStateBehavior.HasTransition())
                {
                    OutputTransitionCodeInner(pseudoStateBehavior);
                    file.RequestNewLineBeforeMoreCode();
                }
            }
        }

        private static bool IsExitingRequired(Vertex source)
        {
            // initial states and entry points are not allowed to exit enclosing state
            if (source is InitialState || source is EntryPoint)
            {
                return false;
            }

            return true;
        }

        private void ExitUntilStateReached(Vertex source, NamedVertex ancestorState)
        {
            bool canUseDirectExit = CanUseDirectExit(ref source, ancestorState);

            if (canUseDirectExit)
            {
                NamedVertex leafActiveState = (NamedVertex)source;
                string sourceExitHandler = mangler.SmFuncTriggerHandler(leafActiveState, TriggerHelper.TRIGGER_EXIT);
                file.AppendLine($"// Avoid exit-while-loop here because we know that the active leaf state is {Vertex.Describe(leafActiveState)} and it is the only state being exited at this point.");
                file.AppendLine($"{sourceExitHandler}(self);");
            }
            else
            {
                string ancestorExitHandler = mangler.SmFuncTriggerHandler(ancestorState, TriggerHelper.TRIGGER_EXIT);

                file.AppendLine($"// At this point, StateSmith doesn't know what the active leaf state is. It could be {Vertex.Describe(source)} or one of its sub states.");
                file.AppendLine($"{mangler.SmFuncExitUpTo}(self, {ancestorExitHandler});  // Exit until we reach {Vertex.Describe(ancestorState)} state.");
            }
        }

        private static bool CanUseDirectExit(ref Vertex source, NamedVertex ancestorState)
        {
            bool canUseDirectExit = false; // if true, requires while loop exit

            if (source is ExitPoint)
            {
                source = source.Parent;  // an exit point is really a part of its parent state.
                canUseDirectExit = true;
            }

            if (source is NamedVertex && source.Children.Any() == false && ancestorState == source.Parent)
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
            bool hasConsumeEventVar = TriggerHelper.IsEvent(triggerName);

            OutputStartOfBehaviorCode(b);
            file.StartCodeBlock();
            {
                MaybeOutputConsumeEventVariable(triggerName, noAncestorHandlesEvent, hasConsumeEventVar);

                OutputAnyActionCode(b);

                MaybeOutputConsumeEventCode(noAncestorHandlesEvent, hasConsumeEventVar);
            }
            OutputEndOfBehaviorCode(b);
        }

        private void OutputEndOfBehaviorCode(Behavior b)
        {
            file.FinishCodeBlock($" // end of behavior for {Vertex.Describe(b.OwningVertex)}");
        }

        private void MaybeOutputConsumeEventCode(bool noAncestorHandlesEvent, bool hasConsumeEventVar)
        {
            if (hasConsumeEventVar)
            {
                file.AppendLine();

                if (noAncestorHandlesEvent)
                {
                    file.AppendLine("// No ancestor handles event. Ignore `consume_event` flag.");
                }
                else
                {
                    file.Append("if (consume_event)");
                    file.StartCodeBlock();
                    {
                        file.AppendLine("// Mark event as handled.");
                        file.AppendLine("self->ancestor_event_handler = NULL;");
                    }
                    file.FinishCodeBlock();
                }
            }
        }

        private void MaybeOutputConsumeEventVariable(string triggerName, bool noAncestorHandlesEvent, bool hasConsumeEventVar)
        {
            if (hasConsumeEventVar)
            {
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
                file.AppendLine("(void)consume_event; // avoid un-used variable compiler warning. StateSmith cannot (yet) detect if behavior action code sets `consume_event`.");
            }
        }
    }
}
