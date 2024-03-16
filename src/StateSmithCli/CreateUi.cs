#nullable enable

using Spectre.Console;
using System.Text;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json;
using NuGet.Configuration;

namespace StateSmith.Runner;

class CreateUi
{
    Settings settings = new();
    private string settingsPath;

    public CreateUi()
    {
        string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!; //TODO consider null
        settingsPath = $"{assemblyFolder}/settings.json";
    }

    public void Run()
    {
        ReadSettingsFromJson();

        Updates();

        StateMachineName();

        AskTargetLanguage();

        StateSmithVersion();

        DiagramType();

        DiagramFileName();

        ScriptFileName();

        DiagramTemplate();

        // TODO:
        // (if version supports it)
        // Generate state machine description markdown file?

        // TODOLOW - help setup vscode script intellisense?

        SaveSettings();

        bool confirmation = AskConfirmation();

        if (!confirmation)
        {
            AnsiConsole.MarkupLine("[red]Aborted.[/]");
            return;
        }

        // TODO write files
    }

    private void ReadSettingsFromJson()
    {
        string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), settingsPath);

        AnsiConsole.MarkupLine($"[yellow]Reading settings from path '{relativePath}'.[/]");

        if (!File.Exists(settingsPath))
        {
            AnsiConsole.MarkupLine($"[yellow]Settings file not found. Using defaults.[/]");
            return;
        }

        try
        {
            var jsonOptions = new JsonSerializerOptions();
            //jsonOptions.IncludeFields = true;
            jsonOptions.PropertyNameCaseInsensitive = true;
            jsonOptions.AllowTrailingCommas = true;
            //jsonOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            jsonOptions.WriteIndented = true;
            settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath), jsonOptions)!; // TODO consider null
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error reading settings file: {ex.Message}[/]. Using defaults.");
        }
    }

    private bool AskConfirmation()
    {
        AddSectionHeader("Confirmation");
        AnsiConsole.MarkupLine($"State Machine Name: [blue]{settings.smName}[/]");
        AnsiConsole.MarkupLine($"Target Language: [blue]{settings.TargetLanguageId}[/]");
        AnsiConsole.MarkupLine($"StateSmith Version: [blue]{settings.StateSmithVersion}[/]");
        AnsiConsole.MarkupLine($"Diagram file: [blue]{settings.diagramFileName}[/]");
        AnsiConsole.MarkupLine($"Script file: [blue]{settings.scriptFileName}[/]");

        if (IsDrawIoSelected())
            AnsiConsole.MarkupLine($"Template: [blue]{settings.DrawIoDiagramTemplateId}[/]");
        else
            AnsiConsole.MarkupLine($"Template: [blue]{settings.PlantUmlDiagramTemplateId}[/]");

        AnsiConsole.MarkupLine("");

        var confirmation = YesNoPrompt("Generate?");
        return confirmation;
    }

    private void ScriptFileName()
    {
        AddSectionHeader("Script File Name/Path");
        AnsiConsole.MarkupLine("Script will be created in this directory unless you specify an absolute or relative path.");
        AnsiConsole.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        AnsiConsole.MarkupLine("");

        string defaultValue = settings.smName + ".csx";
        settings.scriptFileName = Ask("Enter script file name/path", defaultValue);

        settings.scriptFileName = settings.scriptFileName.Replace("$$", settings.smName);
        AnsiConsole.MarkupLine($"Script file name/path: [blue]{settings.scriptFileName}[/]");
    }

    private void DiagramFileName()
    {
        AddSectionHeader("Diagram File Name/Path");
        AnsiConsole.MarkupLine("Diagram will be created in this directory unless you specify an absolute or relative path.");
        AnsiConsole.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        AnsiConsole.MarkupLine("");

        string defaultValue = settings.smName + settings.FileExtension;
        settings.diagramFileName = Ask("Enter diagram file name/path", defaultValue);

        settings.diagramFileName = settings.diagramFileName.Replace("$$", settings.smName);
        AnsiConsole.MarkupLine($"Diagram file name/path: [blue]{settings.diagramFileName}[/]");
    }

    private void DiagramType()
    {
        AddSectionHeader("Diagram Type");
        AnsiConsole.MarkupLine($"Choose the diagram file type you want to use.");
        AnsiConsole.MarkupLine("[grey italic]More info: https://github.com/StateSmith/StateSmith/wiki/draw.io:-file-choice [/]");

        var choices = new List<Item<string>>()
        {
            new Item<string>(id: ".drawio.svg", display: "draw.io SVG [grey]\".drawio.svg\"[/]" ),
            new Item<string>(id: ".drawio",     display: "draw.io XML [grey]\".drawio\"[/]" ),
            new Item<string>(id: ".plantuml",   display: "PlantUML    [grey]\".plantuml\"[/]" ),
        };
        AddRememberedToChoices(choices, id: settings.FileExtension);

        settings.FileExtension = AnsiConsole.Prompt(
                new SelectionPrompt<Item<string>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        AnsiConsole.WriteLine($"Selected {settings.FileExtension}");
    }

    private static bool AddRememberedToChoices<T>(List<Item<T>> choices, T id) where T : notnull
    {
        bool found = false;
        var defaultItem = choices.FirstOrDefault(x => x.Id.Equals(id));
        if (defaultItem != null)
        {
            found = true;
            var topItem = defaultItem.Clone();
            topItem.Display += " [italic grey](remembered)[/]";
            choices.Insert(0, topItem);
        }

        return found;
    }

    private void StateSmithVersion()
    {
        AddSectionHeader("StateSmith Version");

        bool updateAvailable = true;

        AnsiConsole.MarkupLine($"Latest stable: [green]0.9.8-alpha[/]");
        AnsiConsole.MarkupLine("The last version you used with this tool will be the [grey](default)[/].");

        if (updateAvailable)
            AnsiConsole.MarkupLine("[green]Note:[/] you should consider using the latest stable version.");

        AnsiConsole.MarkupLine("[grey italic]Version info: https://github.com/StateSmith/StateSmith/blob/main/CHANGELOG.md [/]");

        AnsiConsole.MarkupLine("");

        settings.StateSmithVersion = Ask("Enter StateSmith version", defaultValue:"0.9.7-alpha");

        AnsiConsole.WriteLine($"Selected {settings.StateSmithVersion}");

        // TODO validate version is valid
    }

    private static string Ask(string prompt, string defaultValue)
    {
        return AnsiConsole.Prompt<string>(
            new TextPrompt<string>(prompt)
                .DefaultValue(defaultValue)
                .DefaultValueStyle("grey")
                );
    }

    private void DiagramTemplate()
    {
        AddSectionHeader("Diagram Template");
        //AnsiConsole.MarkupLine("[grey]?[/]");

        if (IsDrawIoSelected())
        {
            AskDrawIoTemplate();
        }
        else
        {
            AskPlantUmlTemplate();
        }
    }

    private void AskDrawIoTemplate()
    {
        var choices = new List<Item<string>>()
            {
                new Item<string>(id: "drawio-simple-1",    display: "" ),
                new Item<string>(id: "drawio-advanced",    display: "" ),
            };
        var templateTypeName = "draw.io";
        settings.DrawIoDiagramTemplateId = AskTemplate(choices, templateTypeName, settings.DrawIoDiagramTemplateId);
    }

    private void AskPlantUmlTemplate()
    {
        var choices = new List<Item<string>>()
        {
            new Item<string>(id: "plantuml-simple-1",    display: "" ),
        };
        var templateTypeName = "PlantUML";
        settings.PlantUmlDiagramTemplateId = AskTemplate(choices, templateTypeName, settings.PlantUmlDiagramTemplateId);
    }

    private string AskTemplate(List<Item<string>> choices, string templateTypeName, string templateSetting)
    {
        bool found = AddRememberedToChoices(choices, id: templateSetting);

        if (!found)
        {
            AnsiConsole.MarkupLine($"[bold yellow]Warning:[/] Remembered setting `[yellow]{templateSetting}[/]` not found.\n");
        }

        templateSetting = AnsiConsole.Prompt(
                new SelectionPrompt<Item<string>>()
                    .Title($"Select {templateTypeName} template")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        AnsiConsole.MarkupLine($"Selected [blue]{templateSetting}[/]");

        return templateSetting;
    }

    private bool IsDrawIoSelected()
    {
        return settings.FileExtension.Contains(".drawio");
    }

    private void AskTargetLanguage()
    {
        AddSectionHeader("Target Language");

        var choices = new List<Item<TargetLanguageId>>()
        {
            new Item<TargetLanguageId>(id: TargetLanguageId.C,           display: "C99" ),
            new Item<TargetLanguageId>(id: TargetLanguageId.CppC,        display: "C++ [grey](c style, improvements coming)[/]" ),
            new Item<TargetLanguageId>(id: TargetLanguageId.CSharp,      display: "C#" ),
            new Item<TargetLanguageId>(id: TargetLanguageId.JavaScript,  display: "JavaScript" ),
        };

        AddRememberedToChoices(choices, id: settings.TargetLanguageId);

        settings.TargetLanguageId = AnsiConsole.Prompt(
                new SelectionPrompt<Item<TargetLanguageId>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        AnsiConsole.MarkupLine($"Selected [blue]{settings.TargetLanguageId}[/]");
    }

    private void AddSectionHeader(string header)
    {
        AnsiConsole.MarkupLine("");

        var rule = new Rule($"[yellow]{header}[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);
    }

    private void StateMachineName()
    {
        AddSectionHeader("State Machine Name");

        AnsiConsole.MarkupLine("This sets the name of the state machine in the diagram, generated code,");
        AnsiConsole.MarkupLine("and will be used for auto suggesting file names in later steps.");
        AnsiConsole.MarkupLine("[grey]Default shown in (parenthesis).[/]");
        AnsiConsole.MarkupLine("");
        settings.smName = Ask("Enter your state machine name", "MySm");
    }

    private void Updates()
    {
        AddSectionHeader("Updates");

        const string title = "Check for updates?";
        bool checkForUpdates = YesNoPrompt(title);

        if (checkForUpdates)
        {
            CheckForUpdates();
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]Skipping updates...[/]");
        }
    }

    private static bool YesNoPrompt(string title)
    {
        const string yes = "yes";
        return yes == AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(new[] { yes, "no" }));
    }

    private void CheckForUpdates()
    {
        static void WriteLogMessage(string message)
        {
            AnsiConsole.MarkupLine($"[grey]LOG:[/] {message}[grey]...[/]");
        }

        // spinners in action: https://jsfiddle.net/sindresorhus/2eLtsbey/embedded/result/

        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start("[yellow]Initializing warp drive[/]", ctx =>
            {
                ctx.Status("checking for statesmith.ui tool updates");
                Thread.Sleep(1500);
                WriteLogMessage("Connected to nuget.org");

                Thread.Sleep(1500);
                WriteLogMessage("Downloaded stuff");
            });

        // Done
        AnsiConsole.MarkupLine("You are all up to date!");
    }

    private void SaveSettings()
    {
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[grey]Saving settings...[/]");

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(settingsPath, json);

        AnsiConsole.MarkupLine("[grey]Settings saved.[/]");
    }
}
