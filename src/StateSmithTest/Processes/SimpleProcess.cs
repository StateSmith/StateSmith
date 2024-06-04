using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
#nullable enable

namespace StateSmithTest.Processes;

// TODO this is a MODIFIED copy of the class in StateSmith.Cli. It should be moved to a shared project. https://github.com/StateSmith/StateSmith/issues/252
public class SimpleProcess
{
    public const int DefaultLongTimeoutMs = 20000;
    Process cmd = new();

    public string WorkingDirectory = "";
    public string ProgramPath = "";
    public string Args = "";

    public string StdOutput => StdOutputBuf.ToString();
    public string StdError => StdErrorBuf.ToString();

    public StringBuilder StdOutputBuf = new();
    public StringBuilder StdErrorBuf = new();

    public bool throwOnExitCode = true;
    public bool throwOnStdErr = true;

    public static SimpleProcess Build(string programPath, string args, string workingDir)
    {
        return new SimpleProcess
        {
            ProgramPath = programPath,
            Args = args,
            WorkingDirectory = workingDir
        };
    }

    public void RequireLinux()
    {
        bool runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (runningOnWindows)
        {
            string newArgs = $"{ProgramPath} {Args}";
            ProgramPath = "wsl.exe";
            Args = newArgs;
        }
    }

    public void WrapCommandWithBashCOption()
    {
        string newArgs = $"-c \"{ProgramPath} {Args}\"";
        ProgramPath = "/bin/bash";
        Args = newArgs;
    }

    public void RunWithExtraAttemptForWsl(int timeoutMs = DefaultLongTimeoutMs)
    {
        try
        {
            Run(timeoutMs);
        }
        catch (SimpleProcessException)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // WSL2 sometimes fails the first time it is invoked, so just try and run it again
                Run(timeoutMs);
            }
            else
            {
                throw;
            }
        }
    }

    public void Run(int timeoutMs = DefaultLongTimeoutMs, int attempts = 1)
    {
        while (attempts > 0)
        {
            try
            {
                RunInner(timeoutMs);
                return;
            }
            catch (SimpleProcessException)
            {
                attempts--;
                if (attempts <= 0)
                {
                    throw;
                }
            }
        }
    }

    private void RunInner(int timeoutMs)
    {
        SetupCommandAndArgs();
        cmd.StartInfo.WorkingDirectory = WorkingDirectory;

        //Console.WriteLine("cmd.StartInfo.ProgramPath: " + cmd.StartInfo.FileName);
        //Console.WriteLine("cmd.StartInfo.Arguments: " + cmd.StartInfo.Arguments);
        //Console.WriteLine("cmd.StartInfo.WorkingDirectory: " + cmd.StartInfo.WorkingDirectory);

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
        catch (InvalidOperationException e)
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
                throw new InvalidOperationException("Process failure. stdout: " + StdOutput + " stderr: " + StdError, e);
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

        if (throwOnStdErr && StdError.Trim().Length > 0)
        {
            throw new SimpleProcessException(StdError);
        }
    }

    public int GetExitCode()
    {
        return cmd.ExitCode;
    }

    private void SetupCommandAndArgs()
    {
        cmd.StartInfo.FileName = ProgramPath;
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
