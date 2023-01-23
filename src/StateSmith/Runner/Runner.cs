using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateSmith.Compiling;
using System.IO;
using StateSmith.Input.Expansions;
using StateSmith.Input;
using StateSmith.compiler.Visitors;
using StateSmith.Input.PlantUML;
using StateSmith.compiler;
using StateSmith.Common;

#nullable enable

[assembly: CLSCompliant(false)]  

namespace StateSmith.Runner
{
    /// <summary>
    /// Builds a single state machine
    /// </summary>
    public class SmRunner
    {
        private const string ExpansionVarsPath = "self->vars.";
        RunnerSettings settings;
        public CompilerRunner compilerRunner = new();
        ExceptionPrinter exceptionPrinter;

        protected HashSet<string> PlantUmlFileExtensions = new() { ".pu", ".puml", ".plantuml" };
        protected HashSet<string> YedFileExtensions = new() { ".graphml" };
        protected HashSet<string> DrawIoFileEndings = new() { ".drawio.svg", ".drawio", ".dio" };
        
        protected void OutputStageMessage(string message)
        {
            // todo add logger functionality
            Console.WriteLine("StateSmith Runner - " + message);
        }

        public SmRunner(RunnerSettings settings)
        {
            this.settings = settings;
            exceptionPrinter = settings.exceptionPrinter;
        }

        protected void RunInner()
        {
            RunCompiler();
            RunCodeGen();
        }

        public void Run()
        {
            try
            {
                System.Console.WriteLine();
                RunInner();
                OutputStageMessage("finished normally.");
            }
            catch (System.Exception e)
            {
                if (settings.propagateExceptions)
                {
                    throw;
                }

                Environment.ExitCode = -1; // lets calling process know that code gen failed

                exceptionPrinter.PrintException(e);
                DumpErrorDetailsToFile(e);
                OutputStageMessage("finished with failure.");
            }

            System.Console.WriteLine();
        }

        private void DumpErrorDetailsToFile(Exception e)
        {
            var errorDetailFilePath = settings.diagramFile + ".err.txt";
            //errorDetailFilePath = Path.GetFullPath(errorDetailFilePath); // if you want the full path but with resolved "../../"
            errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
            exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
            Console.Error.WriteLine("Additional exception detail dumped to file: " + errorDetailFilePath);
        }

        protected void RunCodeGen()
        {
            Statemachine sm = compilerRunner.sm.ThrowIfNull();
            CodeGenContext codeGenContext = new(sm, settings.renderConfig);
            settings.mangler.SetStateMachine(sm);
            compilerRunner.mangler = settings.mangler;
            codeGenContext.mangler = settings.mangler;
            codeGenContext.style = settings.style;

            foreach (var h in sm.historyStates)
            {
                string actualVarName = codeGenContext.mangler.HistoryVarName(h);
                codeGenContext.expander.AddVariableExpansion(h.stateTrackingVarName, ExpansionVarsPath + actualVarName);

                bool useU8 = false;

                if (useU8)
                {
                    codeGenContext.sm.variables += $"uint8_t {actualVarName};\n";
                    if (h.Behaviors.Count > 255) {
                        throw new VertexValidationException(h, "can't support more than 255 tracked history states right now."); //uint8_t limitation.
                    }
                }
                else
                {
                    codeGenContext.sm.variables += $"enum {compilerRunner.mangler.HistoryVarEnumName(h)} {actualVarName};\n";
                }
            }

            ConfigReader reader = new ConfigReader(codeGenContext.expander, expansionVarsPath: ExpansionVarsPath);
            reader.ReadObject(settings.renderConfig);

            CBuilder cBuilder = new(codeGenContext);
            cBuilder.Generate();

            CHeaderBuilder cHeaderBuilder = new(codeGenContext);
            cHeaderBuilder.Generate();

            string hFileContents = codeGenContext.hFileSb.ToString();
            string cFileContents = codeGenContext.cFileSb.ToString();

            File.WriteAllText($"{settings.outputDirectory}{settings.mangler.HFileName}", hFileContents);
            File.WriteAllText($"{settings.outputDirectory}{settings.mangler.CFileName}", cFileContents);
        }

        private void OutputCompilingDiagramMessage()
        {
            // https://github.com/StateSmith/StateSmith/issues/79
            string filePath = settings.diagramFile;
            if (settings.filePathPrintBase.Trim().Length > 0)
            {
                filePath = Path.GetRelativePath(settings.filePathPrintBase, settings.diagramFile);
            }
            filePath = filePath.Replace('\\', '/');

            OutputStageMessage($"Compiling file: `{filePath}` "
                + ((settings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{settings.stateMachineName}`")
                + "."
            );
        }

        private void RunCompiler()
        {
            OutputCompilingDiagramMessage();
            CompileFile();
            compilerRunner.FinishRunningCompiler();
        }

        private void CompileFile()
        {
            string diagramFile = settings.diagramFile;
            var fileExtension = Path.GetExtension(diagramFile).ToLower();

            if (InputFileIsYedFormat(fileExtension))
            {
                compilerRunner.CompileYedFileNodesToVertices(diagramFile);
            }
            else if (InputFileIsPlantUML(fileExtension))
            {
                compilerRunner.CompilePlantUmlFileNodesToVertices(diagramFile);
            }
            else if (InputFileIsDrawIoFormat(diagramFile))
            {
                compilerRunner.CompileDrawIoFileNodesToVertices(diagramFile);
            }
            else
            {
                var errMessage = $"Unsupported file extension `{fileExtension}`. \n"
                    + $"  - draw.io supports: {string.Join(", ", DrawIoFileEndings)}\n"
                    + $"  - yEd supports: {string.Join(", ", YedFileExtensions)}\n"
                    + $"  - PlantUML supports: {string.Join(", ", PlantUmlFileExtensions)}\n";

                throw new ArgumentException(errMessage);
            }
        }


        private bool InputFileIsPlantUML(string lowerCaseFileExtension)
        {
            return PlantUmlFileExtensions.Contains(lowerCaseFileExtension);
        }

        private bool InputFileIsYedFormat(string lowerCaseFileExtension)
        {
            return YedFileExtensions.Contains(lowerCaseFileExtension);
        }

        private bool InputFileIsDrawIoFormat(string filePath)
        {
            foreach (var matchingEnding in DrawIoFileEndings)
            {
                if (filePath.EndsWith(matchingEnding, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
