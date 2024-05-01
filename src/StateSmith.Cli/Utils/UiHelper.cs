using Spectre.Console;

namespace StateSmith.Cli.Utils;

public class UiHelper
{
    public static bool AskForOverwrite(IAnsiConsole console)
    {
        bool proceed = true;
        console.MarkupLine("[red]Warning![/] File(s) already exist.");

        const string yesOverwriteStr = "yes, overwrite the existing file(s)";
        bool overwrite = yesOverwriteStr == console.Prompt(
                 new SelectionPrompt<string>()
                 .Title("Overwrite existing files?")
                 .AddChoices(new[] { "no", yesOverwriteStr }));

        if (overwrite)
        {
            console.MarkupLine("[yellow]Overwriting existing file(s).[/]");
        }
        else
        {
            console.MarkupLine("[red]Not overwriting file(s).[/]");
            proceed = false;
        }

        return proceed;
    }
}
