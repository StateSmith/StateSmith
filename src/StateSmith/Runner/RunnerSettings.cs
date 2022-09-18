using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;

#nullable enable

namespace StateSmith.Runner
{
    public class RunnerSettings
    {
        public IRenderConfigC renderConfig;
        public string diagramFile;

        public bool propagateExceptions = false;

        /// <summary>
        /// Only required if multiple state machines present in diagram file
        /// </summary>
        public string? stateMachineName;
        public string outputDirectory;

        public CodeStyleSettings style = new();
        public CNameMangler mangler = new();

        public ExceptionPrinter exceptionPrinter = new();

        public RunnerSettings(IRenderConfigC renderConfig, string diagramFile, string outputDirectory)
        {
            this.renderConfig = renderConfig;
            this.diagramFile = diagramFile;
            this.outputDirectory = outputDirectory;
        }
    }
}
