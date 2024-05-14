#nullable enable

using System;
using System.Runtime.InteropServices;

namespace StateSmithTest.Processes.CComp;

/// <summary>
/// Mux is short for multiplexer. This class is a factory for creating the correct compiler based on the OS
/// and environment.
/// </summary>
public class CCompilerMux
{
    readonly IEnvironmentVarProvider envVarProvider;

    public CCompilerMux()
    {
        envVarProvider = new EnvironmentVarProvider();
    }

    public CCompilerMux(IEnvironmentVarProvider envVarProvider)
    {
        this.envVarProvider = envVarProvider;
    }

    public ICompilation Compile(CCompRequest request)
    {
        CCompilerId id = PrepareForCompilation(request);
        return GetCompilation(id, request);
    }

    internal CCompilerId PrepareForCompilation(CCompRequest request)
    {
        string? compilerId = envVarProvider.CompilerId;

        // note: user could have set the env var to an empty string to disable it
        if (string.IsNullOrWhiteSpace(compilerId))
            return GetDefaultCompilerId();
        
        // ignore the compiler path if the compiler id is not set
        request.CompilerPath = envVarProvider.CompilerPath;

        return Enum.Parse<CCompilerId>(compilerId, ignoreCase: true);
    }

    private static CCompilerId GetDefaultCompilerId()
    {
        CCompilerId id;
        bool runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (runningOnWindows)
        {
            id = CCompilerId.WSL_GCC;
        }
        else
        {
            id = CCompilerId.GCC;
        }

        return id;
    }

    private static ICompilation GetCompilation(CCompilerId id, CCompRequest request)
    {
        switch (id)
        {
            case CCompilerId.GCC:
                return new GccCompilation(request);
            case CCompilerId.WSL_GCC:
                return new WslGccCompilation(request);
            case CCompilerId.CLANG:
                return new ClangCompilation(request);
            case CCompilerId.MSVC:
                return new MsvcCompilation(request);
            default:
                throw new ArgumentException($"Compiler ID `{id}` not yet supported");
        }
    }
}
