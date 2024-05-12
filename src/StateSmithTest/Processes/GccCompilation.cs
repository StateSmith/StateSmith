#nullable enable

using System.IO;

namespace StateSmithTest.Processes;

public class GccCompilation : ICompilation
{
    string workingDirectory;

    public GccCompilation(CCompilationRequest request)
    {
        this.workingDirectory = request.WorkingDirectory;
        SimpleProcess process = SetupProcess(request);
        process.Run(timeoutMs: 8000);
    }

    public static SimpleProcess SetupProcess(CCompilationRequest request)
    {
        SimpleProcess process = new()
        {
            ProgramPath = "gcc",
            WorkingDirectory = request.WorkingDirectory
        };

        process.Args += "-Wall ";
        process.Args += string.Join(" ", request.SourceFiles);
        return process;
    }

    public SimpleProcess Run(string runArgs = "")
    {
        SimpleProcess process = new()
        {
            ProgramPath = Path.Combine(workingDirectory, "a.out"),
            WorkingDirectory = workingDirectory,
            Args = runArgs
        };

        process.Run(timeoutMs: 8000);
        return process;
    }
}
