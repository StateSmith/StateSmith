#nullable enable

using System.Collections.Generic;
using System.IO;
using static StateSmithTest.Processes.CCompilationRequest;

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
        AddArgsFromFlags(request.Flags, process);

        process.Args += string.Join(" ", request.SourceFiles);
        return process;
    }

    private static void AddArgsFromFlags(List<FlagId> flags, SimpleProcess process)
    {
        foreach (var flag in flags)
        {
            switch (flag)
            {
                case CCompilationRequest.FlagId.IgnoreUnusedFunctions:
                    process.Args += "-Wno-unused-function ";
                    break;

                default:
                    throw new System.Exception($"Unknown flag: {flag}");
            }
        }
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
