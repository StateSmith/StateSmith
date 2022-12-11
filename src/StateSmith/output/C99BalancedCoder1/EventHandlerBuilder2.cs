#nullable enable

using System;
using System.Linq;
using StateSmith.Common;
using StateSmith.Compiling;
using StateSmith.compiler;
using StateSmith.Input.antlr4;

namespace StateSmith.output.C99BalancedCoder1
{
    public class EventHandlerBuilder2
    {
        private readonly CodeGenContext ctx;
        private readonly Statemachine sm;
        private readonly CNameMangler mangler;
        private readonly OutputFile file;

        public EventHandlerBuilder2(CodeGenContext ctx, OutputFile file)
        {
            this.ctx = ctx;
            sm = ctx.sm;
            mangler = ctx.mangler;
            this.file = file;
        }

        //// todolow - deduplicate
        //private void StartGuardCodeIfNeeded(Behavior b)
        //{
        //    if (b.HasGuardCode())
        //    {
        //        var expandedGuardCode = ExpandingVisitor.ParseAndExpandCode(ctx.expander, b.guardCode);
        //        file.Append($"if ({expandedGuardCode})");
        //        file.StartCodeBlock();
        //    }
        //}

        //// todolow - deduplicate
        //private void FinishGuardCodeIfNeeded(Behavior b)
        //{
        //    if (b.HasGuardCode())
        //    {
        //        file.FinishCodeBlock(" // end of guard code");
        //    }
        //}

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
            if (b.HasGuardCode())
            {
                string sanitizedGuardCode = StringUtils.ReplaceNewLineChars(b.guardCode, "\n//            ");
                file.AppendLines($"// uml guard: {sanitizedGuardCode}");
            }

            if (b.HasActionCode())
            {
                string sanitized = StringUtils.ReplaceNewLineChars(b.actionCode.Trim(), "\n//             ");
                file.AppendLines($"// uml action: {sanitized}");
            }

            if (b.TransitionTarget != null)
            {
                file.AppendLine($"// uml transition target: {Vertex.Describe(b.TransitionTarget)}");
            }
        }

        public void OutputTransitionCode(Behavior behavior)
        {
            if (behavior._transitionTarget == null)
            {
                throw new InvalidOperationException("shouldn't happen");
            }

            Vertex source = behavior.OwningVertex;
            Vertex target = behavior._transitionTarget;

            file.AppendLine($"// {Vertex.Describe(source)} behavior");
            DescribeBehaviorWithUmlComment(behavior);
            file.Append($"if ({GetTransitionGuardCondition(behavior)})");
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
                file.AppendLine("// Enter towards target");
                foreach (var stateToEnter in transitionPath.toEnter)
                {
                    if (stateToEnter is NamedVertex namedVertexToEnter)
                    {
                        var enterHandler = mangler.SmFuncTriggerHandler(namedVertexToEnter, TriggerHelper.TRIGGER_ENTER);
                        file.AppendLine($"{enterHandler}(self);");
                    }
                }

                file.RequestNewLineBeforeMoreCode();

                if (target is PseudoStateVertex pseudoStateVertex)
                {
                    OutputTransitionsForPseudoState(pseudoStateVertex);
                }
                else if (target is NamedVertex namedVertexTarget)
                {
                    InitialState? initialState = namedVertexTarget.Children.OfType<InitialState>().FirstOrDefault();

                    if (initialState != null)
                    {
                        OutputTransitionsForPseudoState(initialState);
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
            file.FinishCodeBlock($" // end of behavior for {Vertex.Describe(behavior.OwningVertex)}");
        }

        private void OutputAnyActionCode(Behavior behavior)
        {
            if (behavior.HasActionCode())
            {
                var expandedAction = ExpandingVisitor.ParseAndExpandCode(ctx.expander, behavior.actionCode);
                file.AppendLines($"{expandedAction}");
            }
        }

        private void OutputTransitionsForPseudoState(PseudoStateVertex pseudoState)
        {
            foreach (Behavior initialStateBehavior in pseudoState.Behaviors)
            {
                if (initialStateBehavior.HasTransition())
                {
                    OutputTransitionCode(initialStateBehavior);
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

        //private void OutputExitUpToSelf(NamedVertex state)
        //{
        //    file.Append("// Exit up to node that is the transition source");
        //    ExitUntilStateReached(state);
        //}

        private void ExitUntilStateReached(Vertex source, NamedVertex ancestorState)
        {
            bool canUseDirectExit = CanUseDirectExit(ref source, ancestorState);

            if (canUseDirectExit)
            {
                NamedVertex leafActiveState = (NamedVertex)source;
                string sourceExitHandler = mangler.SmFuncTriggerHandler(leafActiveState, TriggerHelper.TRIGGER_EXIT);
                file.AppendLine($"// Optimize away while-exit-loop because we know that the active leaf state is {Vertex.Describe(leafActiveState)} and it is exiting directly to its parent {Vertex.Describe(ancestorState)}.");
                file.AppendLine($"{sourceExitHandler}(self);");
            }
            else
            {
                // We don't know what the leaf active state is (some child of source). We must use 
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

            var behaviorsWithTrigger = TriggerHelper.GetBehaviorsWithTrigger(state, triggerName).OrderBy((b) => b.order);
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

            file.AppendLine($"// {Vertex.Describe(b.OwningVertex)} behavior");
            DescribeBehaviorWithUmlComment(b);
            file.Append($"if ({GetTransitionGuardCondition(b)})");
            file.StartCodeBlock();
            {
                MaybeOutputConsumeEventVariable(triggerName, noAncestorHandlesEvent, hasConsumeEventVar);

                OutputAnyActionCode(b);

                MaybeOutputConsumeEventCode(noAncestorHandlesEvent, hasConsumeEventVar);
            }
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
