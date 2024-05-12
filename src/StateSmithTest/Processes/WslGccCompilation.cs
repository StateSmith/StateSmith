#nullable enable

namespace StateSmithTest.Processes;

public class WslGccCompilation : ICompilation
{
    string workingDirectory;

    public WslGccCompilation(CCompilationRequest request)
    {
        this.workingDirectory = request.WorkingDirectory;

        SimpleProcess process = GccCompilation.SetupProcess(request);
        process.RequireLinux();
        process.Run(timeoutMs: 8000, attempts: 2);
    }

    public SimpleProcess Run(string runArgs = "")
    {
        SimpleProcess process = new()
        {
            ProgramPath = "./a.out",
            WorkingDirectory = workingDirectory,
            Args = runArgs
        };

        // Wrapping with Bash is required so that we can run the a.out file.
        // We can't execute the a.out file directly because we are running in Windows and a.out is a Linux executable
        process.WrapCommandWithBashCOption();
        process.RequireLinux(); // required for bash

        process.Run(timeoutMs: 8000);
        return process;
    }
}
