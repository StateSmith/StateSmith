#nullable enable

using System.Collections.Generic;
using StateSmith.Compiling;
using System.Linq;
using StateSmith.Common;
using System;

namespace StateSmith.output.C99BalancedCoder1
{
    public class CBuilder
    {
        private readonly CodeGenContext ctx;
        private readonly Statemachine sm;
        private readonly CNameMangler mangler;
        private readonly OutputFile file;
        EventHandlerBuilder eventHandlerBuilder;

        public CBuilder(CodeGenContext ctx)
        {
            this.ctx = ctx;
            sm = ctx.sm;
            mangler = ctx.mangler;
            file = new(ctx, ctx.cFileSb);

            eventHandlerBuilder = new(ctx, file);
        }

        public void Generate()
        {
            file.AddLinesIfNotBlank(ctx.renderConfig.CFileTop);

            file.AddLine($"#include \"{mangler.HFileName}\"");
            file.AddLinesIfNotBlank(ctx.renderConfig.CFileIncludes);
            file.AddLine("#include <stdbool.h> // required for `consume_event` flag");

            //file.AddLine($"#include \"stddef.h\"");
            //file.AddLine($"#include \"stdint.h\"");
            file.AddLine($"#include <string.h> // for memset");
            file.AddLine();

            OutputTriggerHandlerPrototypes();

            OutputFuncCtor();
            OutputFuncStart();
            OutputFuncDispatchEvent();
            OutputFuncStateIdToString();

            OutputTriggerHandlers();
        }

        internal void OutputTriggerHandlerPrototypes()
        {
            List<NamedVertex> namedVertices = sm.GetNamedVerticesCopy();
            
            foreach (var state in namedVertices)
            {
                OutputTriggerHandlerPrototype(state, TriggerHelper.TRIGGER_ENTER);
                OutputTriggerHandlerPrototype(state, TriggerHelper.TRIGGER_EXIT);

                string[] eventNames = GetEvents(state).ToArray();
                Array.Sort(eventNames);

                foreach (var eventName in eventNames)
                {
                    OutputTriggerHandlerPrototype(state, eventName);
                }

                file.AddLine();
            }
        }

        /// <summary>
        /// Ctor is short for constructor
        /// </summary>
        internal void OutputFuncCtor()
        {
            file.Append($"void {mangler.SmFuncCtor}({mangler.SmStructTypedefName}* self)");
            file.StartCodeBlock();
            file.AddLine("memset(self, 0, sizeof(*self));");
            file.FinishCodeBlock();
            file.AddLine();
        }

        internal void OutputFuncStateIdToString()
        {
            file.Append($"const char* {mangler.SmFuncToString}(const enum {mangler.SmStateEnum} id)");  // todolow share function prototype string generation
            file.StartCodeBlock();
            {
                file.Append("switch (id)");
                file.StartCodeBlock();
                {
                    foreach (var state in ctx.sm.GetNamedVerticesCopy())
                    {
                        file.AddLine($"case {mangler.SmStateEnumValue(state)}: return \"{mangler.SmStateToString(state)}\";");
                    }
                    file.AddLine("default: return \"?\";");
                }
                file.FinishCodeBlock();
            }
            file.FinishCodeBlock();
            file.AddLine();
        }

        internal void OutputFuncStart()
        {
            file.Append($"void {mangler.SmFuncStart}({mangler.SmStructTypedefName}* self)");
            file.StartCodeBlock();
            file.AddLine("ROOT_enter(self);");

            var initialState = sm.Children.OfType<InitialState>().Single();
            var initial_transition = initialState.Behaviors.Single();

            if (initial_transition.TransitionTarget == null)
            {
                throw new VertexValidationException(initialState, "Initial states must have a single transition");
            }

            NamedVertex transitionTarget = (NamedVertex)initial_transition.TransitionTarget;
            eventHandlerBuilder.OutputCodeForNonSelfTransition(sm, transitionTarget);
            file.FinishCodeBlock();
            file.AddLine();
        }

        internal void OutputFuncDispatchEvent()
        {
            file.Append($"void {mangler.SmFuncDispatchEvent}({mangler.SmStructTypedefName}* self, enum {mangler.SmEventEnum} event_id)");
            file.StartCodeBlock();
            file.AddLine($"{mangler.SmFuncTypedef} behavior_func = self->current_event_handlers[event_id];");
            file.AddLine();
            //file.AddLine("self->current_event = event_id;");
            //file.AddLine();
            file.Append("while (behavior_func != NULL)");
            {
                file.StartCodeBlock();
                file.AddLine("self->ancestor_event_handler = NULL;");
                file.AddLine("behavior_func(self);");
                file.AddLine("behavior_func = self->ancestor_event_handler;");
                file.FinishCodeBlock();
            }
            file.FinishCodeBlock();
            file.AddLine();
        }

        internal void OutputTriggerHandlers()
        {
            List<NamedVertex> namedVertices = sm.GetNamedVerticesCopy();
            
            foreach (var state in namedVertices)
            {
                
                file.AddLine("////////////////////////////////////////////////////////////////////////////////");
                file.AddLine($"// event handlers for state {mangler.SmStateName(state)}");
                file.AddLine("////////////////////////////////////////////////////////////////////////////////");
                file.AddLine();

                OutputFuncStateEnter(state);
                OutputFuncStateExit(state);

                // fixme output event handlers
                string[] eventNames = GetEvents(state).ToArray();
                Array.Sort(eventNames);

                foreach (var eventName in eventNames)
                {
                    OutputTriggerHandlerSignature(state, eventName);
                    file.StartCodeBlock();
                    {
                        eventHandlerBuilder.OutputStateBehaviorsForTrigger(state, eventName);
                    }
                    file.FinishCodeBlock();
                    file.AddLine();
                }

                file.AddLine();
            }
        }

        internal void OutputFuncStateEnter(NamedVertex state)
        {
            OutputTriggerHandlerSignature(state, TriggerHelper.TRIGGER_ENTER);

            file.StartCodeBlock();
            {
                file.AddLine($"// setup trigger/event handlers");
                string stateExitHandlerName = mangler.SmFuncTriggerHandler(state, TriggerHelper.TRIGGER_EXIT);
                file.AddLine($"self->current_state_exit_handler = {stateExitHandlerName};");

                string[] eventNames = GetEvents(state).ToArray();
                Array.Sort(eventNames);

                foreach (var eventName in eventNames)
                {
                    string handlerName = mangler.SmFuncTriggerHandler(state, eventName);
                    string eventEnumValueName = mangler.SmEventEnumValue(eventName);
                    file.AddLine($"self->current_event_handlers[{eventEnumValueName}] = {handlerName};");
                }

                eventHandlerBuilder.OutputStateBehaviorsForTrigger(state, TriggerHelper.TRIGGER_ENTER);
            }
            file.FinishCodeBlock();
            file.AddLine();
        }

        internal void OutputFuncStateExit(NamedVertex state)
        {
            OutputTriggerHandlerSignature(state, TriggerHelper.TRIGGER_EXIT);

            file.StartCodeBlock();
            {
                eventHandlerBuilder.OutputStateBehaviorsForTrigger(state, TriggerHelper.TRIGGER_EXIT);

                if (state.Parent == null)
                {
                    file.AddLine($"// State machine root is a special case. It cannot be exited.");
                    file.AddLine($"(void)self;  // nothing to see here compiler. move along!");
                }
                else
                {
                    file.AddLine($"// adjust function pointers for this state's exit");
                    string parentExitHandler = mangler.SmFuncTriggerHandler((NamedVertex)state.Parent, TriggerHelper.TRIGGER_EXIT);
                    file.AddLine($"self->current_state_exit_handler = {parentExitHandler};");

                    string[] eventNames = GetEvents(state).ToArray();
                    Array.Sort(eventNames);

                    foreach (var eventName in eventNames)
                    {
                        string eventEnumValueName = mangler.SmEventEnumValue(eventName);
                        var ancestor = state.FirstAncestorThatHandlesEvent(eventName);
                        if (ancestor != null)
                        {
                            string handlerName = mangler.SmFuncTriggerHandler(ancestor, eventName);
                            file.AddLine($"self->current_event_handlers[{eventEnumValueName}] = {handlerName};  // the next ancestor that handles this event is {mangler.SmStateName(ancestor)}");
                        }
                        else
                        {
                            file.AddLine($"self->current_event_handlers[{eventEnumValueName}] = NULL;  // no ancestor listens to this event");
                        }
                    }
                }
            }
            file.FinishCodeBlock();
            file.AddLine();
        }

        internal void OutputTriggerHandlerPrototype(NamedVertex state, string eventName)
        {
            OutputTriggerHandlerSignature(state, eventName);
            file.AddLine(";");
        }

        internal void OutputTriggerHandlerSignature(NamedVertex state, string eventName)
        {
            file.Append($"static void {mangler.SmFuncTriggerHandler(state, eventName)}({mangler.SmStructTypedefName}* self)");
        }

        /// <summary>
        /// These do NOT include entry and exit triggers
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static HashSet<string> GetEvents(NamedVertex state)
        {
            var triggerNames = TriggerHelper.GetStateTriggers(state);
            triggerNames.Remove(TriggerHelper.TRIGGER_ENTER);
            triggerNames.Remove(TriggerHelper.TRIGGER_EXIT);
            return triggerNames;
        }
    }
}
