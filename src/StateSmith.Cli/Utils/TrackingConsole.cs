using Spectre.Console;

namespace StateSmith.Cli.Utils;

public class TrackingConsole
{
    public bool Accessed = false;
    private IAnsiConsole console;

    public TrackingConsole(IAnsiConsole console)
    {
        this.console = console;
    }

    public IAnsiConsole Get()
    {
        Accessed = true;
        return console;
    }
}
