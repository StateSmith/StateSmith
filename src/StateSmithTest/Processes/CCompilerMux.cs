#nullable enable

using System.Runtime.InteropServices;

namespace StateSmithTest.Processes;

/// <summary>
/// Mux is short for multiplexer. This class is a factory for creating the correct compiler based on the OS
/// and environment.
/// </summary>
public class CCompilerMux
{
    public static ICompilation Compile(CCompilationRequest request)
    {
        bool runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        ICompilation compilation;

        // TODO - support environment variables for the compiler to use and paths involved

        if (runningOnWindows)
        {
            compilation = new WslGccCompilation(request);
        }
        else
        {
            compilation = new GccCompilation(request);
        }

        return compilation;
    }
}
