using Spectre.Console;
using System;

namespace StateSmith.Cli.Run;

public class RunConsole
{
    IAnsiConsole _console;

    public RunConsole(IAnsiConsole console)
    {
        _console = console;
    }

    public void AddMildHeader(string header)
    {
        _console.MarkupLine("");

        var rule = new Rule($"[blue]{header}[/]")
        {
            Justification = Justify.Left
        };
        _console.Write(rule);
    }

    public void QuietMarkupLine(string message)
    {
        MarkupLine($"[grey]{message}[/]");
    }

    public void WarnMarkupLine(string message)
    {
        MarkupLine($"[yellow]{message}[/]");
    }

    public void ErrorMarkupLine(string message)
    {
        MarkupLine($"[red]{message}[/]");
    }

    public void WriteLine(string message)
    {
        _console.WriteLine(message);
    }

    public void WriteException(Exception exception)
    {         
        _console.WriteException(exception);
    }

    public void MarkupLine(string message)
    {
        _console.MarkupLine(message);
    }
}
