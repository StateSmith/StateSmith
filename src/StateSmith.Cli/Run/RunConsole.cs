using Spectre.Console;
using System;

namespace StateSmith.Cli.Run;

public class RunConsole
{
    IAnsiConsole _console;
    bool _silent = false;

    public RunConsole(IAnsiConsole console)
    {
        _console = console;
    }

    public RunConsole CloneWithoutSettings()
    {
        return new RunConsole(_console);
    }

    public void SetSilent(bool silent)
    {
        _silent = silent;
    }

    public IAnsiConsole GetIAnsiConsole()
    {
        return _console;
    }

    public void AddMildHeader(string header)
    {
        if (_silent)
            return;

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
        if (_silent)
            return;

        _console.WriteLine(message);
    }

    public void WriteException(Exception exception)
    {
        if (_silent)
            return;

        _console.WriteException(exception);
    }

    public void MarkupLine(string message)
    {
        if (_silent)
            return;

        _console.MarkupLine(message);
    }

    public void Markup(string message)
    {
        if (_silent)
            return;

        _console.Markup(message);
    }
}
