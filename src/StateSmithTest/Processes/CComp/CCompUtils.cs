#nullable enable

using System.IO;

namespace StateSmithTest.Processes.CComp;

public class CCompUtils
{
    public static string GetCompilerPathOrDefault(CCompRequest request, string commandDefault)
    {
        // trim is important because the user could have set the env var to an empty string to disable it
        if (request.CompilerPath != null && request.CompilerPath.Trim().Length > 0)
        {
            return request.CompilerPath;
        }
        else
        {
            return commandDefault;
        }
    }

    public static void AddGccClangArgs(SimpleProcess process, CCompRequest request)
    {
        process.Args += "-Wall ";

        foreach (var flag in request.Flags)
        {
            switch (flag)
            {
                case CCompRequest.FlagId.IgnoreUnusedFunctions:
                    process.Args += "-Wno-unused-function ";
                    break;

                default:
                    throw new System.Exception($"Unknown flag: {flag}");
            }
        }
    }

    /// <summary>
    /// Runs a default Unix executable named "a.out" in the working directory.
    /// </summary>
    /// <param name="runArgs"></param>
    /// <param name="workingDirectory"></param>
    /// <returns></returns>
    public static SimpleProcess RunDefaultUnixExecutable(string runArgs, string workingDirectory)
    {
        SimpleProcess process = SetupRunDefaultUnixExecutable(runArgs, workingDirectory);
        process.Run(timeoutMs: 8000);
        return process;
    }

    public static SimpleProcess SetupRunDefaultUnixExecutable(string runArgs, string workingDirectory)
    {
        return new()
        {
            ProgramPath = Path.Combine(workingDirectory, "a.out"),
            WorkingDirectory = workingDirectory,
            Args = runArgs
        };
    }

    public static SimpleProcess SetupDefaultProcess(CCompRequest request, string command)
    {
        SimpleProcess process = new()
        {
            ProgramPath = GetCompilerPathOrDefault(request, command),
            WorkingDirectory = request.WorkingDirectory
        };

        return process;
    }

    public static void AddSourceFilesArgs(SimpleProcess process, CCompRequest request)
    {
        process.Args += string.Join(" ", request.SourceFiles);
    }

    public static void GccClangCompile(CCompRequest request, string command)
    {
        SimpleProcess process = GccClangSetup(request, command);
        process.Run(timeoutMs: 8000);
    }

    public static SimpleProcess GccClangSetup(CCompRequest request, string command)
    {
        SimpleProcess process = SetupDefaultProcess(request, command: command);
        AddGccClangArgs(process, request);
        AddSourceFilesArgs(process, request);
        return process;
    }
}
