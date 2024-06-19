using Spectre.Console;

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

    public void WriteLine(string message)
    {
        _console.WriteLine(message);
    }

    public void MarkupLine(string message)
    {
        _console.MarkupLine(message);
    }
}
