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

#nullable enable

namespace StateSmith.Runner
{
    public class RunnerSettings
    {
        public IRenderConfigC renderConfig;
        public string diagramFile;

        /// <summary>
        /// Only required if multiple state machines present in diagram file
        /// </summary>
        public string? stateMachineName;
        public string outputDirectory;

        public CodeStyleSettings style = new();
        public CNameMangler mangler = new();

        public RunnerSettings(IRenderConfigC renderConfig, string diagramFile, string outputDirectory)
        {
            this.renderConfig = renderConfig;
            this.diagramFile = diagramFile;
            this.outputDirectory = outputDirectory;
        }
    }

    /// <summary>
    /// Builds a single state machine
    /// </summary>
    public class SmRunner
    {
        RunnerSettings settings;
        Statemachine sm = new("non_null_dummy");
        Compiler compiler = new();

        public SmRunner(RunnerSettings settings)
        {
            this.settings = settings;
        }

        public void Run()
        {
            compiler.CompileFile(settings.diagramFile);
            compiler.SetupRoots();
            compiler.SupportParentAlias();
            compiler.Validate();

            compiler.SimplifyInitialStates();
            compiler.DefaultToDoEventIfNoTrigger();
            compiler.FinalizeTrees();
            compiler.Validate();

            if (settings.stateMachineName != null)
            {
                sm = (Statemachine)compiler.GetVertex(settings.stateMachineName);
            }
            else
            {
                sm = compiler.rootVertices.OfType<Statemachine>().Single();
            }

            //ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);


            CodeGenContext codeGenContext = new(sm, settings.renderConfig);
            settings.mangler.SetStateMachine(sm);
            codeGenContext.mangler = settings.mangler;
            codeGenContext.style = settings.style;

            ConfigReader reader = new ConfigReader(codeGenContext.expander, expansionVarsPath: "self->vars.");
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
    }
}
