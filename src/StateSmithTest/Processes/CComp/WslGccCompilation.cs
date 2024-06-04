#nullable enable

using System.IO;

namespace StateSmithTest.Processes.CComp;

public class WslGccCompilation : ICompilation
{
    readonly string workingDirectory;

    /// <summary>
    /// Compiles the source files using GCC in WSL.
    /// </summary>
    /// <param name="request"></param>
    public WslGccCompilation(CCompRequest request)
    {
        workingDirectory = request.WorkingDirectory;
        SimpleProcess process = CCompUtils.GccClangSetup(request, command: "gcc");
        process.RequireLinux();
        process.RunWithExtraAttemptForWsl(timeoutMs: 8000);
    }

    public SimpleProcess RunExecutable(string runArgs = "")
    {
        SimpleProcess process = new()
        {
            ProgramPath = "./a.out", // NOTE! This is different from the Unix version which executes a.out directly. See below for notes.
            WorkingDirectory = workingDirectory,
            Args = runArgs
        };

        // Wrapping with Bash is required so that we can run the a.out file.
        // We can't execute the a.out file directly because we are running in Windows and a.out is a Linux executable
        process.WrapCommandWithBashCOption();
        process.RequireLinux(); // required for bash

        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
        return process;
    }
}
