using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
#nullable enable

namespace StateSmithTest.Processes;

public class SimpleProcess
{
    public string WorkingDirectory = "";
    public string CommandText = "";

    public string StdOutput = "";
    public string StdError = "";

    public bool throwOnExitCode = true;
}

public class BashRunnerException : InvalidOperationException
{
    public BashRunnerException(string? message) : base(message)
    {
    }
}

public class BashRunner
{
    public static void RunCommand(SimpleProcess simpleProcess, int timeoutMs = 3000)
    {
        try
        {
            RunCommandSimple(simpleProcess, timeoutMs);
        }
        catch (BashRunnerException)
        {
            // WSL2 seems to fail the first time it is invoked, so just try and run it again
            RunCommandSimple(simpleProcess, timeoutMs);
        }
    }

    public static void RunCommandSimple(SimpleProcess simpleProcess, int timeoutMs = 3000)
    {
        Process cmd = new();
        cmd.StartInfo.WorkingDirectory = simpleProcess.WorkingDirectory;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            cmd.StartInfo.FileName = "wsl.exe";
            cmd.StartInfo.Arguments = $"{simpleProcess.CommandText} ";
        }
        else
        {
            cmd.StartInfo.FileName = "/bin/bash";
            cmd.StartInfo.Arguments = $"-c \"{simpleProcess.CommandText}\"";
        }

        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.RedirectStandardError = true;
        cmd.Start();

        ReadUntilTimeoutOrFinished(simpleProcess, cmd, timeoutMs);

        if (cmd.ExitCode != 0)
        {
            throw new BashRunnerException("Exit code: " + cmd.ExitCode + ".\nOutput:\n" + simpleProcess.StdOutput + "\nError Output:\n" + simpleProcess.StdError);
        }
    }

    /// <summary>
    /// If process prints a lot to stdout/err, need to read in chunks or else `WaitForExit()` will fail.
    /// Seems like a buffering issue.
    /// </summary>
    private static void ReadUntilTimeoutOrFinished(SimpleProcess simpleProcess, Process cmd, int timeoutMs)
    {
        bool success = false;

        int timeLeftMs = timeoutMs;
        while (timeLeftMs > 0 && !success)
        {
            simpleProcess.StdError += cmd.StandardError.ReadToEnd();    // stderr must be read before stdout in case of error or thread hangs on windows
            simpleProcess.StdOutput += cmd.StandardOutput.ReadToEnd();

            int waitMs = Math.Clamp(timeoutMs/5, 1, 100);
            if (cmd.WaitForExit(waitMs))
            {
                success = true;
            }
            timeLeftMs -= waitMs; // not exact timing, but that's OK for this right now
        }

        if (!success)
        {
            // `WaitForExit()` doesn't always work when calling wsl.exe and gcc prints only to stderr. Works fine on Linux though.
            // That's why we have to specify a timeout and force stop on Windows.
            cmd.Kill(entireProcessTree: true);
            cmd.WaitForExit();
        }
    }
}
