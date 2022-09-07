#nullable enable

using System.Runtime.CompilerServices;
using StateSmith.Compiling;

namespace StateSmith.output.C99BalancedCoder1
{
    public class CHeaderBuilder
    {
        private readonly CodeGenContext ctx;
        private readonly CEnumBuilder cEnumBuilder;
        private readonly Statemachine sm;
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
            
            file.AddLinesIfNotBlank(ctx.renderConfig.HFileTop);
            
            file.AddLine("#pragma once");
            file.AddLinesIfNotBlank(ctx.renderConfig.HFileIncludes);
            file.AddLine();

            cEnumBuilder.OutputEventIdCode();
            file.AddLine();

            cEnumBuilder.OutputStateIdCode();
            file.AddLine();

            OutputTypedefs(file);
            file.AddLine();

            OutputStructDefinition(file);
            file.AddLine();

            OutputFunctionPrototypes(file);
            file.AddLine();
        }

        internal void OutputTypedefs(OutputFile file)
        {
            var structName = mangler.SmStructName;
            string structTypedefName = mangler.SmStructTypedefName;
            string funcTypedef = mangler.SmFuncTypedef;

            // ex: `typedef struct MySm MySm;`
            file.AddLine($"typedef struct {structName} {structTypedefName};");

            // ex: `typedef void (*MySmFunc)(MySm* sm);`
            file.AddLine($"typedef void (*{funcTypedef})({structTypedefName}* sm);");
        }

        internal void OutputStructDefinition(OutputFile file)
        {
            // file.AddLine($"// State machine structure.");
            file.Append($"struct {mangler.SmStructName}");
            file.StartCodeBlock();

            // block just for viewing
            {
                file.AddLine($"// Used internally by state machine. Feel free to inspect, but don't modify.");
                file.AddLine($"enum {mangler.SmStateEnum} state_id;");
                //file.AddLine($"enum {mangler.SmEventEnum} current_event;");

                file.AddLine();
                file.AddLine($"// Used internally by state machine. Don't modify.");
                file.AddLine($"{mangler.SmFuncTypedef} ancestor_event_handler;");

                file.AddLine();
                file.AddLine($"// Used internally by state machine. Don't modify.");
                file.AddLine($"{mangler.SmFuncTypedef} current_event_handlers[{mangler.SmEventEnumCount}];");

                file.AddLine();
                file.AddLine($"// Used internally by state machine. Don't modify.");
                file.AddLine($"{mangler.SmFuncTypedef} current_state_exit_handler;");

                if (ctx.renderConfig.VariableDeclarations.Trim().Length > 0)
                {
                    file.AddLine();
                    file.AddLine("// User variables. Can be used for inputs, outputs, user variables...");
                    file.Append("struct");
                    file.StartCodeBlock();
                    {
                        var lines = StringUtils.SplitIntoLines(ctx.renderConfig.VariableDeclarations.Trim());
                        foreach (var line in lines)
                        {
                            file.AddLine(line);
                        }
                    }
                    file.FinishCodeBlock(" vars;");

                }

            }

            file.FinishCodeBlock(";");
        }

        internal void OutputFunctionPrototypes(OutputFile file)
        {
            file.AddLine("// State machine constructor. Must be called before start or dispatch event functions. Not thread safe.");
            file.AddLine($"void {mangler.SmFuncCtor}({mangler.SmStructTypedefName}* self);");

            file.AddLine();
            file.AddLine("// Starts the state machine. Must be called before dispatching events. Not thread safe.");
            file.AddLine($"void {mangler.SmFuncStart}({mangler.SmStructTypedefName}* self);");

            file.AddLine();
            file.AddLine("// Dispatches an event to the state machine. Not thread safe.");
            file.AddLine($"void {mangler.SmFuncDispatchEvent}({mangler.SmStructTypedefName}* self, enum {mangler.SmEventEnum} event_id);");

            file.AddLine();
            file.AddLine("// Converts a state id to a string. Thread safe.");
            file.AddLine($"const char* {mangler.SmFuncToString}(const enum {mangler.SmStateEnum} id);");
        }
    }
}
