#nullable enable

using System.Runtime.CompilerServices;
using StateSmith.SmGraph;

namespace StateSmith.Output.C99BalancedCoder1
{
    public class CHeaderBuilder
    {
        private readonly CodeGenContext ctx;
        private readonly CEnumBuilder cEnumBuilder;
        private readonly StateMachine sm;
        private readonly CNameMangler mangler;

        public CHeaderBuilder(CodeGenContext ctx)
        {
            this.ctx = ctx;
            cEnumBuilder = new CEnumBuilder(ctx);
            sm = ctx.sm;
            mangler = ctx.mangler;
        }

        public void Generate()
        {
            OutputFile file = new(ctx, ctx.hFileSb);
            
            file.AppendLinesIfNotBlank(ctx.renderConfig.HFileTop);
            
            file.AppendLine("#pragma once");
            file.AppendLine("#include <stdint.h>");  // required at least for history state functionality
            file.AppendLinesIfNotBlank(ctx.renderConfig.HFileIncludes);
            file.AppendLine();

            cEnumBuilder.OutputEventIdCode();
            file.AppendLine();

            cEnumBuilder.OutputStateIdCode();
            file.AppendLine();

            foreach (var h in sm.historyStates)
            {
                cEnumBuilder.OutputHistoryIdCode(h);
                file.AppendLine();
            }

            OutputTypedefs(file);
            file.AppendLine();

            OutputStructDefinition(file);
            file.AppendLine();

            OutputFunctionPrototypes(file);
            file.AppendLine();
        }

        internal void OutputTypedefs(OutputFile file)
        {
            var structName = mangler.SmStructName;
            string structTypedefName = mangler.SmStructTypedefName;
            string funcTypedef = mangler.SmFuncTypedef;

            // ex: `typedef struct MySm MySm;`
            file.AppendLine($"typedef struct {structName} {structTypedefName};");

            // ex: `typedef void (*MySmFunc)(MySm* sm);`
            file.AppendLine($"typedef void (*{funcTypedef})({structTypedefName}* sm);");
        }

        internal void OutputStructDefinition(OutputFile file)
        {
            // file.AddLine($"// State machine structure.");
            file.Append($"struct {mangler.SmStructName}");
            file.StartCodeBlock();
            {
                file.AppendLine($"// Used internally by state machine. Feel free to inspect, but don't modify.");
                file.AppendLine($"enum {mangler.SmStateEnum} state_id;");
                //file.AddLine($"enum {mangler.SmEventEnum} current_event;");

                file.AppendLine();
                file.AppendLine($"// Used internally by state machine. Don't modify.");
                file.AppendLine($"{mangler.SmFuncTypedef} ancestor_event_handler;");

                file.AppendLine();
                file.AppendLine($"// Used internally by state machine. Don't modify.");
                file.AppendLine($"{mangler.SmFuncTypedef} current_event_handlers[{mangler.SmEventEnumCount}];");

                file.AppendLine();
                file.AppendLine($"// Used internally by state machine. Don't modify.");
                file.AppendLine($"{mangler.SmFuncTypedef} current_state_exit_handler;");

                if (IsVarsStructNeeded())
                {
                    file.AppendLine();
                    file.AppendLine("// User variables. Can be used for inputs, outputs, user variables...");
                    file.Append("struct");
                    file.StartCodeBlock();
                    {
                        var combinedVars = ctx.sm.variables + ctx.renderConfig.VariableDeclarations.Trim();

                        var lines = StringUtils.SplitIntoLines(combinedVars);
                        foreach (var line in lines)
                        {
                            file.AppendLine(line);
                        }
                    }
                    file.FinishCodeBlock(" vars;");

                }
            }
            file.FinishCodeBlock(";");
        }

        internal bool IsVarsStructNeeded()
        {
            if (ctx.sm.variables.Length > 0)
            {
                return true;
            }

            return IsVariableDeclarationsNonEmpty();
        }

        private bool IsVariableDeclarationsNonEmpty()
        {
            return StringUtils.RemoveCCodeComments(ctx.renderConfig.VariableDeclarations).Trim().Length > 0;
        }

        internal void OutputFunctionPrototypes(OutputFile file)
        {
            file.AppendLine("// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.");
            file.AppendLine($"void {mangler.SmFuncCtor}({mangler.SmStructTypedefName}* self);");

            file.AppendLine();
            file.AppendLine("// Starts the state machine. Must be called before dispatching events. Not thread safe.");
            file.AppendLine($"void {mangler.SmFuncStart}({mangler.SmStructTypedefName}* self);");

            file.AppendLine();
            file.AppendLine("// Dispatches an event to the state machine. Not thread safe.");
            file.AppendLine($"void {mangler.SmFuncDispatchEvent}({mangler.SmStructTypedefName}* self, enum {mangler.SmEventEnum} event_id);");

            file.AppendLine();
            file.AppendLine("// Converts a state id to a string. Thread safe.");
            file.AppendLine($"const char* {mangler.SmFuncToString}(const enum {mangler.SmStateEnum} id);");
        }
    }
}
