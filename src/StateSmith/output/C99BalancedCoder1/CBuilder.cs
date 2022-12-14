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
            file.AppendLinesIfNotBlank(ctx.renderConfig.CFileTop);

            file.AppendLine($"#include \"{mangler.HFileName}\"");
            file.AppendLinesIfNotBlank(ctx.renderConfig.CFileIncludes);
            file.AppendLine("#include <stdbool.h> // required for `consume_event` flag");

            //file.AddLine($"#include \"stddef.h\"");
            //file.AddLine($"#include \"stdint.h\"");
            file.AppendLine($"#include <string.h> // for memset");
            file.AppendLine();

            OutputTriggerHandlerPrototypes();
            OutputHelperPrototypes();
            file.AppendLine();
            file.AppendLine();

            OutputFuncCtor();
            
            OutputExitUpToFunction();

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

                file.AppendLine();
            }
        }

        internal void OutputHelperPrototypes()
        {
            file.AppendLine($"static void {mangler.SmFuncExitUpTo}({mangler.SmStructTypedefName}* self, const {mangler.SmFuncTypedef} desired_state_exit_handler);");
        }

        /// <summary>
        /// Ctor is short for constructor
        /// </summary>
        internal void OutputFuncCtor()
        {
            file.Append($"void {mangler.SmFuncCtor}({mangler.SmStructTypedefName}* self)");
            file.StartCodeBlock();
            file.AppendLine("memset(self, 0, sizeof(*self));");
            file.FinishCodeBlock();
            file.AppendLine();
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
                        file.AppendLine($"case {mangler.SmStateEnumValue(state)}: return \"{mangler.SmStateToString(state)}\";");
                    }
                    file.AppendLine("default: return \"?\";");
                }
                file.FinishCodeBlock(forceNewLine: true);
            }
            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }

        internal void OutputFuncStart()
        {
            file.Append($"void {mangler.SmFuncStart}({mangler.SmStructTypedefName}* self)");
            file.StartCodeBlock();
            file.AppendLine("ROOT_enter(self);");

            var initialState = sm.Children.OfType<InitialState>().Single();

            var getToInitialStateBehavior = new Behavior(sm, initialState);

            eventHandlerBuilder.OutputTransitionCode(getToInitialStateBehavior);

            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }

        internal void OutputExitUpToFunction()
        {
            file.Append($"static void {mangler.SmFuncExitUpTo}({mangler.SmStructTypedefName}* self, const {mangler.SmFuncTypedef} desired_state_exit_handler)");
            file.StartCodeBlock();

            file.Append($"while (self->current_state_exit_handler != desired_state_exit_handler)");
            file.StartCodeBlock();
            {
                file.AppendLine("self->current_state_exit_handler(self);");
            }
            file.FinishCodeBlock(forceNewLine: true);

            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }

        internal void OutputFuncDispatchEvent()
        {
            file.Append($"void {mangler.SmFuncDispatchEvent}({mangler.SmStructTypedefName}* self, enum {mangler.SmEventEnum} event_id)");
            file.StartCodeBlock();
            file.AppendLine($"{mangler.SmFuncTypedef} behavior_func = self->current_event_handlers[event_id];");
            file.AppendLine();
            //file.AddLine("self->current_event = event_id;");
            //file.AddLine();
            file.Append("while (behavior_func != NULL)");
            {
                file.StartCodeBlock();
                file.AppendLine("self->ancestor_event_handler = NULL;");
                file.AppendLine("behavior_func(self);");
                file.AppendLine("behavior_func = self->ancestor_event_handler;");
                file.FinishCodeBlock(forceNewLine: true);
            }
            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }

        internal void OutputTriggerHandlers()
        {
            List<NamedVertex> namedVertices = sm.GetNamedVerticesCopy();
            
            foreach (var state in namedVertices)
            {
                
                file.AppendLine("////////////////////////////////////////////////////////////////////////////////");
                file.AppendLine($"// event handlers for state {mangler.SmStateName(state)}");
                file.AppendLine("////////////////////////////////////////////////////////////////////////////////");
                file.AppendLine();

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
                    file.FinishCodeBlock(forceNewLine: false);
                    file.AppendLine();
                }

                file.AppendLine();
            }
        }

        internal void OutputFuncStateEnter(NamedVertex state)
        {
            OutputTriggerHandlerSignature(state, TriggerHelper.TRIGGER_ENTER);

            file.StartCodeBlock();
            {
                file.AppendLine($"// setup trigger/event handlers");
                string stateExitHandlerName = mangler.SmFuncTriggerHandler(state, TriggerHelper.TRIGGER_EXIT);
                file.AppendLine($"self->current_state_exit_handler = {stateExitHandlerName};");

                string[] eventNames = GetEvents(state).ToArray();
                Array.Sort(eventNames);

                foreach (var eventName in eventNames)
                {
                    string handlerName = mangler.SmFuncTriggerHandler(state, eventName);
                    string eventEnumValueName = mangler.SmEventEnumValue(eventName);
                    file.AppendLine($"self->current_event_handlers[{eventEnumValueName}] = {handlerName};");
                }

                file.RequestNewLineBeforeMoreCode();
                eventHandlerBuilder.OutputStateBehaviorsForTrigger(state, TriggerHelper.TRIGGER_ENTER);
            }
            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }

        internal void OutputFuncStateExit(NamedVertex state)
        {
            OutputTriggerHandlerSignature(state, TriggerHelper.TRIGGER_EXIT);

            file.StartCodeBlock();
            {
                eventHandlerBuilder.OutputStateBehaviorsForTrigger(state, TriggerHelper.TRIGGER_EXIT);

                if (state.Parent == null)
                {
                    file.AppendLine($"// State machine root is a special case. It cannot be exited.");
                    file.AppendLine($"(void)self;  // nothing to see here compiler. move along!");
                }
                else
                {
                    file.AppendLine($"// adjust function pointers for this state's exit");
                    string parentExitHandler = mangler.SmFuncTriggerHandler((NamedVertex)state.Parent, TriggerHelper.TRIGGER_EXIT);
                    file.AppendLine($"self->current_state_exit_handler = {parentExitHandler};");

                    string[] eventNames = GetEvents(state).ToArray();
                    Array.Sort(eventNames);

                    foreach (var eventName in eventNames)
                    {
                        string eventEnumValueName = mangler.SmEventEnumValue(eventName);
                        var ancestor = state.FirstAncestorThatHandlesEvent(eventName);
                        if (ancestor != null)
                        {
                            string handlerName = mangler.SmFuncTriggerHandler(ancestor, eventName);
                            file.AppendLine($"self->current_event_handlers[{eventEnumValueName}] = {handlerName};  // the next ancestor that handles this event is {mangler.SmStateName(ancestor)}");
                        }
                        else
                        {
                            file.AppendLine($"self->current_event_handlers[{eventEnumValueName}] = NULL;  // no ancestor listens to this event");
                        }
                    }
                }
            }
            file.FinishCodeBlock(forceNewLine: true);
            file.AppendLine();
        }

        internal void OutputTriggerHandlerPrototype(NamedVertex state, string eventName)
        {
            OutputTriggerHandlerSignature(state, eventName);
            file.AppendLine(";");
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
