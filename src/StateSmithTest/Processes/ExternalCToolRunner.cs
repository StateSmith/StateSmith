#nullable enable

using System.Collections.Generic;

namespace StateSmithTest.Processes;

public class ExternalCToolRunner
{
    public class Args
    {
        public required string CompilerPath;
        public required string WorkingDirectory;
        public required List<string> SourceFiles;
        public string AdditionalCompilerArgs = "";
        public bool RunWithBash = false;
    }

    SimpleProcess process = new();
    Args args;

    public ExternalCToolRunner(Args args)
    {
        this.args = args;
    }

    public void Run()
    {
        process.ProgramPath = args.CompilerPath;
        process.WorkingDirectory = args.WorkingDirectory;
        
        if (args.CompilerPath.EndsWith("gcc"))
        {
            SetupGcc();
        }
        else
        {
             throw new System.Exception("Unknown compiler: " + args.CompilerPath);
        }

        StartProcess();
    }

    private void StartProcess()
    {
        if (args.RunWithBash)
        {
            BashRunner.RunCommand(process);
        }
        else
        {
            process.Run(timeoutMs: 8000);
        }
    }

    private void SetupGcc()
    {
        process.Args += "-Wall ";
        process.Args += args.AdditionalCompilerArgs + " ";
        process.Args += SourceFilesString();
    }

    private string SourceFilesString()
    {
        return string.Join(" ", args.SourceFiles);
    }
}
