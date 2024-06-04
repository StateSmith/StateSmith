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
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    private static void AddArgs(SimpleProcess process, CCompRequest request)
    {
        process.Args += "/Wall ";
        process.Args += "/wd4820 "; // Warning C4820   padding added after data member
        process.Args += $"/Fe:{createdExecutableName} "; // output file name

        // TODO https://github.com/StateSmith/StateSmith/issues/262
        // TODO - need to add include path to avoid errors like: `main.c(1): fatal error C1083: Cannot open include file: 'stdio.h': No such file or directory`
        // TODO - need to add library path to avoid errors like: `LINK : fatal error LNK1104: cannot open file 'LIBCMT.lib'`

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

        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
        return process;
    }
}
