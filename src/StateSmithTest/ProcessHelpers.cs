using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
#nullable enable

namespace StateSmithTest.Processes;

public class SimpleProcess
{
    public string WorkingDirectory = "";
    public string CommandText = "";

    public string StdOutput => StdOutputBuf.ToString();
    public string StdError => StdErrorBuf.ToString();

    public StringBuilder StdOutputBuf = new();
    public StringBuilder StdErrorBuf = new();

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

        bool runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (runningOnWindows)
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

        cmd.OutputDataReceived += (sender, args) => simpleProcess.StdOutputBuf.Append(args.Data).Append('\n');
        cmd.ErrorDataReceived += (sender, args) => simpleProcess.StdErrorBuf.Append(args.Data).Append('\n');

        // If modifying this code, make sure you read all of the below to avoid deadlocks.
        // https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?view=net-7.0
        cmd.Start();
        cmd.BeginOutputReadLine();
        cmd.BeginErrorReadLine();

        if (!cmd.WaitForExit(timeoutMs))
        {
            cmd.Kill(entireProcessTree: true);
            cmd.WaitForExit();
        }

        cmd.WaitForExitAsync().Wait();  // required so that async std err and output operations finish before we return from method.

        if (cmd.ExitCode != 0)
        {
            throw new BashRunnerException("Exit code: " + cmd.ExitCode + ".\nOutput:\n" + simpleProcess.StdOutput + "\nError Output:\n" + simpleProcess.StdError);
        }
    }
}
