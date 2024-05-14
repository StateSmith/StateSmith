#nullable enable

using System;
using System.Runtime.InteropServices;

namespace StateSmithTest.Processes.CComp;

public class ClangCompilation : ICompilation
{
    readonly string workingDirectory;

    /// <summary>
    /// Compiles the source files using Clang.
    /// </summary>
    /// <param name="request"></param>
    public ClangCompilation(CCompRequest request)
    {
        workingDirectory = request.WorkingDirectory;
        CCompUtils.GccClangCompile(request, command: "clang");
    }

    public SimpleProcess RunExecutable(string runArgs = "")
    {
        bool runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (runningOnWindows)
            throw new NotImplementedException("clang on windows isn't supported yet. Want to help?");

        return CCompUtils.RunDefaultUnixExecutable(runArgs: runArgs, workingDirectory: workingDirectory);
    }
}
