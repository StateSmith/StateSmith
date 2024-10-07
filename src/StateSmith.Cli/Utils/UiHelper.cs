using Spectre.Console;
using System;

namespace StateSmith.Cli.Utils;

public class UiHelper
{
    public static bool YesNoPrompt(IAnsiConsole console, string title, string yesText = "yes")
    {
        return yesText == console.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(new[] { yesText, "no" }));
    }

    public static bool NoYesPrompt(IAnsiConsole console, string title, string noText = "no")
    {
        const string yes = "yes";
        return yes == console.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(new[] { noText, yes }));
    }

    public static void AddSectionLeftHeader(IAnsiConsole console, string header, string color = "yellow")
    {
        console.MarkupLine("");

        var rule = new Rule($"[{color}]{header}[/]")
        {
            Justification = Justify.Left
        };
        console.Write(rule);
    }

    public static T Prompt<T>(IAnsiConsole _console, string title, UiItem<T>[] choices) where T : notnull
    {
        return _console.Prompt(
                new SelectionPrompt<UiItem<T>>()
                    .Title(title)
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;
    }
}
