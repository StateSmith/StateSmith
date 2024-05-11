using System.Runtime.InteropServices;
#nullable enable

namespace StateSmithTest.Processes;

public class BashRunner
{
    public static void RunCommand(SimpleProcess simpleProcess, int timeoutMs = 8000, bool throwOnStdErr = true)
    {
        simpleProcess.SetupToRunWithBash();

        try
        {
            simpleProcess.Run(timeoutMs);
        }
        catch (SimpleProcessException)
        {
            // WSL2 seems to fail the first time it is invoked, so just try and run it again
            simpleProcess.Run(timeoutMs);
        }

        if (throwOnStdErr && simpleProcess.StdError.Trim().Length > 0)
        {
            throw new SimpleProcessException(simpleProcess.StdError);
        }
    }
}
