#nullable enable

using System.IO;

namespace StateSmithTest.Processes.CComp;

public class MsvcCompilation : ICompilation
{
    const string createdExecutableName = "a.exe";
    readonly string workingDirectory;

    /// <summary>
    /// Compiles source files using MSVC.
    /// </summary>
    /// <param name="request"></param>
    public MsvcCompilation(CCompRequest request)
    {
        workingDirectory = request.WorkingDirectory;
        SimpleProcess process = CCompUtils.SetupDefaultProcess(request, command: "cl");

        AddArgs(process, request);
        CCompUtils.AddSourceFilesArgs(process, request);
        process.Run(timeoutMs: 8000);
    }

    private static void AddArgs(SimpleProcess process, CCompRequest request)
    {
        process.Args += "/Wall ";
        process.Args += "/wd4820 "; // Warning C4820   padding added after data member
        process.Args += $"/Fe {createdExecutableName} "; // output file name

        foreach (var flag in request.Flags)
        {
            switch (flag)
            {
                case CCompRequest.FlagId.IgnoreUnusedFunctions:
                    process.Args += ""; // not needed for MSVC
                    break;

                default:
                    throw new System.Exception($"Unknown flag: {flag}");
            }
        }
    }

    public SimpleProcess RunExecutable(string runArgs = "")
    {
        SimpleProcess process = new()
        {
            ProgramPath = Path.Combine(workingDirectory, createdExecutableName),
            WorkingDirectory = workingDirectory,
            Args = runArgs
        };

        process.Run(timeoutMs: 8000);
        return process;
    }
}
