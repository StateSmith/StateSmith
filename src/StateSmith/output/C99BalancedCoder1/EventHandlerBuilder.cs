#nullable enable

using System;
using StateSmith.Compiling;
using StateSmith.Common;
using StateSmith.Input.antlr4;
using System.Linq;

namespace StateSmith.output.C99BalancedCoder1
{
    // FIXME test
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

        // TODO refactor large method into smaller ones. The logic is a bit repetitive/unclear too.
        public void OutputStateBehaviorsForTrigger(NamedVertex state, string triggerName)
        {
            NamedVertex? nextHandlingState = null;
            bool noAncestorHandlesEvent;

            if (TriggerHelper.IsEvent(triggerName))
            {
                file.AppendLine($"// setup handler for next ancestor that listens to `{triggerName}` event");
                file.Append("self->ancestor_event_handler = ");
                nextHandlingState = state.FirstAncestorThatHandlesEvent(triggerName);
                noAncestorHandlesEvent = nextHandlingState == null;
                if (nextHandlingState != null)
                {
                    file.FinishLine($"{mangler.SmFuncTriggerHandler(nextHandlingState, triggerName)};");
                }
                else
                {
                    file.FinishLine($"NULL; // no ancestor state handles `{triggerName}` event");
                }
            }
            noAncestorHandlesEvent = nextHandlingState == null;
    
            var behaviorsWithTrigger = TriggerHelper.GetBehaviorsWithTrigger(state, triggerName).OrderBy((b) => b.order);
            foreach (var b in behaviorsWithTrigger)
            {
                bool requiredConsumeEventCode = TriggerHelper.IsEvent(triggerName);
                bool forceConsumeEvent = b.HasTransition(); // if has transition, event MUST be consumed. No variable option to override.
                bool hasConsumeEventVar = requiredConsumeEventCode && !forceConsumeEvent; // if has transition, event MUST be consumed. No variable option to override.

                file.AppendLine();
                file.AppendLine("// state behavior:");
                file.StartCodeBlockHere();
                {
                    if (forceConsumeEvent)
                    {
                        file.AppendLine("// Note: no `consume_event` variable possible here because of state transition. The event must be consumed.");
                    }
                    else if (hasConsumeEventVar)
                    {
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
                        
                        if (noAncestorHandlesEvent)
                        {
                            file.AppendLine("// note: no ancestor consumes this event, but we output `bool consume_event` anyway because a user's design might rely on it.");
                        }
                        
                        file.AppendLine();
                    }

                    DescribeBehaviorWithUmlComment(b);

                    StartGuardCodeIfNeeded(b);
                    OutputAnyActionCode(b);

                    OutputAnyTransitionCode(state, triggerName, b);

                    if (requiredConsumeEventCode)
                    {
                        file.AppendLine();

                        if (forceConsumeEvent)
                        {
                            OutputConsumeEventCode(nextHandlingState, becauseOfTransition: true);
                        }
                        else
                        {
                            // hasConsumeEventVar must be true
                            if (!hasConsumeEventVar)
                            {
                                throw new InvalidOperationException("This shouldn't happen");
                            }
                            file.Append("if (consume_event)");
                            file.StartCodeBlock();
                            {
                                OutputConsumeEventCode(nextHandlingState, becauseOfTransition: false);
                            }
                            file.FinishCodeBlock();
                        }
                    }

                    if (b.HasTransition())
                    {
                        file.AppendLine($"return; // event processing immediately stops when a transition occurs. No other behaviors for this state are checked.");
                    }

                    FinishGuardCodeIfNeeded(b);
                }
                file.FinishCodeBlock(" // end of behavior code");
            }
        }

        private void OutputConsumeEventCode(NamedVertex? nextHandlingState, bool becauseOfTransition)
        {
            file.Append("// Mark event as handled.");
            if (becauseOfTransition)
            {
                file.AppendWithoutIndent(" Required because of transition.");
            }
            file.FinishLine();

            if (nextHandlingState != null)
            {
                file.AppendLine("self->ancestor_event_handler = NULL;");
            }
            else
            {
                file.AppendLine("// self->ancestor_event_handler = NULL; // already done at top of function");
            }
        }

        private void OutputAnyTransitionCode(NamedVertex state, string triggerName, Behavior b)
        {
            if (b.HasTransition() == false)
            {
                return;
            }

            if (b.HasActionCode())
            {
                file.AppendLine();
            }

            ThrowIfHasTransitionOnEnterOrExitHandler(state, triggerName, b);

            NamedVertex target = (NamedVertex)b.TransitionTarget;   // will need to be updated when we allow transitioning to other types of vertices

            if (target == state)
            {
                file.AppendLine("// self transition");
            }
            OutputCodeForTransition(state, target);
        }

        internal void OutputCodeForTransition(NamedVertex state, NamedVertex target, bool skipStateExiting = false)
        {
            file.Append("// Transition to target state " + target.Name);
            file.StartCodeBlock(forceNewLine: true);
            {
                var transitionPath = state.FindTransitionPathTo(target);
                if (skipStateExiting)
                {
                    file.AppendLine($"// No need exit any states in this handler.");
                }
                else
                {
                    OutputPathExitToLcaCode((NamedVertex)transitionPath.leastCommonAncestor);
                }

                file.AppendLine();
                file.AppendLine("// Enter towards target");
                foreach (var stateToEnter in transitionPath.toEnter)
                {
                    var enterHandler = mangler.SmFuncTriggerHandler((NamedVertex)stateToEnter, TriggerHelper.TRIGGER_ENTER);
                    file.AppendLine($"{enterHandler}(self);");
                }

                file.AppendLine();
                file.AppendLine("// update state_id");
                file.AppendLine($"self->state_id = {mangler.SmStateEnumValue(target)};");
            }
            file.FinishCodeBlock(" // end of transition code");
        }

        /// <summary>
        /// LCA means Least Common Ancestor
        /// </summary>
        private void OutputPathExitToLcaCode(NamedVertex leastCommonAncestor)
        {
            file.AppendLine($"// First, exit up to Least Common Ancestor {mangler.SmStateName(leastCommonAncestor)}.");
            string lcaExitHandler = mangler.SmFuncTriggerHandler(leastCommonAncestor, TriggerHelper.TRIGGER_EXIT);
            file.Append($"while (self->current_state_exit_handler != {lcaExitHandler})");
            file.StartCodeBlock();
            {
                file.AppendLine("self->current_state_exit_handler(self);");
            }
            file.FinishCodeBlock();
        }

        internal void OutputAnyActionCode(Behavior b)
        {
            if (b.HasActionCode())
            {
                var expandedAction = ExpandingVisitor.ParseAndExpandCode(ctx.expander, b.actionCode);
                file.AppendLines($"{expandedAction}");
            }
        }

        private void FinishGuardCodeIfNeeded(Behavior b)
        {
            if (b.HasGuardCode())
            {
                file.FinishCodeBlock(" // end of guard code");
            }
        }

        private void StartGuardCodeIfNeeded(Behavior b)
        {
            if (b.HasGuardCode())
            {
                var expandedGuardCode = ExpandingVisitor.ParseAndExpandCode(ctx.expander, b.guardCode);
                file.Append($"if ({expandedGuardCode})");
                file.StartCodeBlock();
            }
        }

        private static void ThrowIfHasTransitionOnEnterOrExitHandler(NamedVertex state, string triggerName, Behavior b)
        {
            if (TriggerHelper.IsEnterExitTrigger(triggerName))
            {
                if (b.TransitionTarget != null)
                {
                    throw new BehaviorValidationException(b, "Transitions cannot have an enter or exit trigger.");
                }
            }
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
                NamedVertex target = (NamedVertex)b.TransitionTarget;   // will need to be updated when we allow transitioning to other types of vertices
                file.AppendLine($"// uml transition target: {target.Name}");
            }
        }
    }
}
