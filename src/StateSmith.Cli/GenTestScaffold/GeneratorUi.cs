using Spectre.Console;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StateSmith.Cli.GenTestScaffold;

public interface ITemplateGenerator
{
    TargetLanguageId LanguageId { get; }
    string DisplayId { get; }
    string? ShortDescription { get; }
    string SuggestFileName(GeneratorData data);
    string GenerateFileContent(GeneratorData data);
}

public class GeneratorData
{
    public string StateMachineName = "";
    public string FirstTestEventName = "";

    /// <summary>
    /// Will be null if there isn't a clear initial first state in the diagram.
    /// </summary>
    public string? FirstTestStateName;
}

public class GeneratorUi
{
    private IAnsiConsole console;
    private Settings settings;
    private List<ITemplateGenerator> generators = new();
    private IEnumerable<ITemplateGenerator> availableGenerators;

    public GeneratorUi(IAnsiConsole console, Settings settings)
    {
        this.console = console;
        this.settings = settings;
        AddGenerator(new Template.JsJest());

        availableGenerators = generators.Where(g => g.LanguageId == settings.TargetLanguageId);
    }

    public void AddGenerator(ITemplateGenerator generator)
    {
        generators.Add(generator);
    }

    public void AskGenForNewProject()
    {
        if (!availableGenerators.Any())
            return;

        UiHelper.AddSectionLeftHeader(console, "Generate Test Scaffold");

        if (!UiHelper.YesNoPrompt(console, "Would you like to generate a test scaffold?"))
        {
            PrintAbortedMessage();
            return;
        }

        List<UiItem<ITemplateGenerator>> choices = new();
        foreach (var g in availableGenerators)
        {
            choices.Add(new UiItem<ITemplateGenerator>(g, display: g.DisplayId + $"[grey] - {g.ShortDescription}[/]"));
        }

        ITemplateGenerator generator = UiHelper.Prompt(console, "Select test scaffold template", choices);

        GeneratorData data = new()
        {
            StateMachineName = settings.smName,
            FirstTestStateName = "OFF",
            FirstTestEventName = "INCREASE",
        };

        string suggestedFileName = generator.SuggestFileName(data);
        string filePath = UiHelper.Ask(console, "File path to generate to?", defaultValue: suggestedFileName);

        if (File.Exists(filePath))
        {
            if (UiHelper.AskForOverwrite(console) == false)
            {
                PrintAbortedMessage();
                return;
            }
        }

        string fileContents = generator.GenerateFileContent(data);
        File.WriteAllText(filePath, fileContents);

        console.MarkupLine("[green]Test scaffold generated successfully.[/]");
    }

    private void PrintAbortedMessage()
    {
        console.MarkupLine("[grey]Not generating test scaffold.[/]");
    }
}
