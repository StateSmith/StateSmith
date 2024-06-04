using Spectre.Console;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// TODO : This is a copy of the SimpleProcess class from StateSmithTest. It should be moved to a shared library. https://github.com/StateSmith/StateSmith/issues/252
namespace StateSmith.Cli.Utils;

public class SimpleProcess
{
    Process cmd = new();

    public string WorkingDirectory = "";

    /// <summary>
    /// This splits the command and args at the first space char. If you need fancier (like a space in a path to the executable), use <see cref="SpecificCommand"/> instead.
    /// </summary>
    public string CommandAndArgs = "";

    /// <summary>
    /// If this set to a non-empty string, it and <see cref="SpecificArgs"/> will be used instead of <see cref="CommandAndArgs"/>.
    /// </summary>
    public string SpecificCommand = "";
    public string SpecificArgs = "";

    public string StdOutput => StdOutputBuf.ToString();
    public string StdError => StdErrorBuf.ToString();

    public StringBuilder StdOutputBuf = new();
    public StringBuilder StdErrorBuf = new();

    public bool throwOnExitCode = true;

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
        cmd.BeginOutputReadLine();
        cmd.BeginErrorReadLine();

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
            throw new BashRunnerException(message);
        }
    }

    public int GetExitCode()
    {
        return cmd.ExitCode;
    }

    private void SetupCommandAndArgs()
    {
        if (SpecificCommand.Length > 0)
        {
            cmd.StartInfo.FileName = SpecificCommand;
            cmd.StartInfo.Arguments = SpecificArgs;
        }
        else
        {
            var split = CommandAndArgs.Split(' ', count: 1);
            cmd.StartInfo.FileName = split[0];

            if (split.Length > 1)
                cmd.StartInfo.Arguments = split[1];
        }

        if (cmd.StartInfo.FileName.Length == 0)
            throw new BashRunnerException("Command needs to be specified");
    }

    public void AddStdOutListener(DataReceivedEventHandler handler)
    {
        cmd.OutputDataReceived += handler;
    }

    public void AddStdErrListener(DataReceivedEventHandler handler)
    {
        cmd.ErrorDataReceived += handler;
    }

    public void EnableEchoToTerminal(IAnsiConsole console)
    {
        AddStdOutListener((sender, args) =>
        {
            if (args.Data != null)
                console.WriteLine(args.Data);
        });
        AddStdErrListener((sender, args) =>
        {
            if (args.Data != null)
                console.MarkupLine($"[red]{args.Data.EscapeMarkup()}[/]");
        });
    }

    public void EnableEchoToTerminal()
    {
        AddStdOutListener((sender, args) => Console.WriteLine(args.Data));
        AddStdErrListener((sender, args) => Console.Error.WriteLine(args.Data));
    }
}

public class BashRunnerException : InvalidOperationException
{
    public BashRunnerException(string? message) : base(message)
    {
    }
}

