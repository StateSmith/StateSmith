#nullable enable

namespace StateSmithTest.Processes.CComp;

public class GccCompilation : ICompilation
{
    readonly string workingDirectory;

    /// <summary>
    /// Compiles source files using GCC assuming a Unix environment.
    /// </summary>
    /// <param name="request"></param>
    public GccCompilation(CCompRequest request)
    {
        workingDirectory = request.WorkingDirectory;
        CCompUtils.GccClangCompile(request, command: "gcc");
    }

    public SimpleProcess RunExecutable(string runArgs = "")
    {
        return CCompUtils.RunDefaultUnixExecutable(runArgs: runArgs, workingDirectory: workingDirectory);
    }
}
