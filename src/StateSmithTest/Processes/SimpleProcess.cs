using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
#nullable enable

namespace StateSmithTest.Processes;

// TODO this is a MODIFIED copy of the class in StateSmith.Cli. It should be moved to a shared project. https://github.com/StateSmith/StateSmith/issues/252
public class SimpleProcess
{
    Process cmd = new();

    public string WorkingDirectory = "";

    /// <summary>
    /// If this set to a non-empty string, it and <see cref="Args"/> will be used instead of <see cref="CommandAndArgs"/>.
    /// </summary>
    public string Command = "";
    public string Args = "";

    public string StdOutput => StdOutputBuf.ToString();
    public string StdError => StdErrorBuf.ToString();

    public StringBuilder StdOutputBuf = new();
    public StringBuilder StdErrorBuf = new();

    public bool throwOnExitCode = true;

    public static SimpleProcess Build(string specificCommand, string specificArgs, string workingDir)
    {
        return new SimpleProcess
        {
            Command = specificCommand,
            Args = specificArgs,
            WorkingDirectory = workingDir
        };
    }

    public void NestCommand(string command, string args)
    {
        string newArgs = $"{args} {Command} {Args}";
        Command = command;
        Args = newArgs;
    }

    public void SetupToRunWithBash()
    {
        bool runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (runningOnWindows)
        {
            this.NestCommand("wsl.exe", $"");
        }
        else
        {
            this.NestCommand("/bin/bash", $"-c");
        }
    }

    public void Run(int timeoutMs)
    {
        SetupCommandAndArgs();

        cmd.StartInfo.WorkingDirectory = WorkingDirectory;

        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.RedirectStandardError = true;

        // add our listeners that append to buf
        AddStdOutListener((sender, args) => this.StdOutputBuf.Append(args.Data).Append('\n'));
        AddStdErrListener((sender, args) => this.StdErrorBuf.Append(args.Data).Append('\n'));

        // If modifying this code, make sure you read all of the below to avoid deadlocks.
        // https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?view=net-7.0
        cmd.Start();

        // Improve error reporting for failed commands.
        // https://github.com/StateSmith/StateSmith/issues/271
        try
        {
            // NOTE! The below begin methods will throw an InvalidOperationException if the command failed.
            cmd.BeginErrorReadLine();
            cmd.BeginOutputReadLine();
        }
        catch (InvalidOperationException)
        {
            // checking for exit code doesn't work here as the process is still running.
            // wait a bit.
            if (cmd.WaitForExit(milliseconds: 1000) && cmd.ExitCode != 0)
            {
                // let below code handle the error.
            }
            else
            {
                // we don't know what happened, so throw.
                throw;
            }
        }

        bool killedProcess = false;

        if (!cmd.WaitForExit(timeoutMs))
        {
            cmd.Kill(entireProcessTree: true);
            cmd.WaitForExit();
            killedProcess = true;
        }

        cmd.WaitForExitAsync().Wait();  // required so that async std err and output operations finish before we return from method.

        if (throwOnExitCode && cmd.ExitCode != 0)
        {
            string message = "Exit code: " + cmd.ExitCode + ".\nOutput:\n" + this.StdOutput + "\nError Output:\n" + this.StdError;
            if (killedProcess)
                message = $"Timed out {timeoutMs}ms and killed process. " + message;
            throw new SimpleProcessException(message);
        }
    }

    public int GetExitCode()
    {
        return cmd.ExitCode;
    }

    private void SetupCommandAndArgs()
    {
        cmd.StartInfo.FileName = Command;
        cmd.StartInfo.Arguments = Args;
    }

    public void AddStdOutListener(DataReceivedEventHandler handler)
    {
        cmd.OutputDataReceived += handler;
    }

    public void AddStdErrListener(DataReceivedEventHandler handler)
    {
        cmd.ErrorDataReceived += handler;
    }

    public void EnableEchoToTerminal()
    {
        AddStdOutListener((sender, args) => Console.WriteLine(args.Data));
        AddStdErrListener((sender, args) => Console.Error.WriteLine(args.Data));
    }
}
