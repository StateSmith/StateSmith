using Spectre.Console;

namespace StateSmith.Cli.Utils;

public class Prompter : IPrompter
{
    private readonly IAnsiConsole _console;

    public Prompter(IAnsiConsole console)
    {
        this._console = console;
    }

    public bool AskForOverwrite()
    {
        bool proceed = true;
        _console.MarkupLine("[red]Warning![/] File(s) already exist.");

        const string yesOverwriteStr = "yes, overwrite the existing file(s)";
        bool overwrite = yesOverwriteStr == _console.Prompt(
                 new SelectionPrompt<string>()
                 .Title("Overwrite existing files?")
                 .AddChoices(new[] { "no", yesOverwriteStr }));

        if (overwrite)
        {
            _console.MarkupLine("[yellow]Overwriting existing file(s).[/]");
        }
        else
        {
            _console.MarkupLine("[red]Not overwriting file(s).[/]");
            proceed = false;
        }

        return proceed;
    }

    public bool YesNoPrompt(string title, string yesText = "yes")
    {
        return UiHelper.YesNoPrompt(_console, title, yesText);
    }

    public bool NoYesPrompt(string title, string noText = "no")
    {
        return UiHelper.NoYesPrompt(_console, title, noText);
    }

    public T Prompt<T>(string title, UiItem<T>[] choices) where T : notnull
    {
        return UiHelper.Prompt(_console, title, choices);
    }
}
