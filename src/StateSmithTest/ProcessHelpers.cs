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

public class BashRunner
{
    public static void RunCommand(SimpleProcess simpleProcess, int timeoutMs = 3000)
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

        if (!cmd.WaitForExit(timeoutMs))
        {
            // `WaitForExit()` doesn't always work when calling wsl.exe and gcc prints only to stderr. Works fine on Linux though.
            // That's why we have to specify a timeout and force stop on Windows.
            cmd.Kill(entireProcessTree: true);
            cmd.WaitForExit();
        }
        simpleProcess.StdOutput = cmd.StandardOutput.ReadToEnd();
        simpleProcess.StdError = cmd.StandardError.ReadToEnd();

        if (cmd.ExitCode != 0)
        {
            throw new InvalidOperationException("Exit code: " + cmd.ExitCode + ".\nOutput:\n" + simpleProcess.StdOutput + "\nError Output:\n" + simpleProcess.StdError);
        }
    }
}
