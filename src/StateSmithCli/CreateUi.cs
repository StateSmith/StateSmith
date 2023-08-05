#nullable enable

using Spectre.Console;
using System.Text;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Runner;

// statesmith --create

enum TargetLanguageId
{
    C,
    CppC,
    CSharp,
    JavaScript,
}

class CreateUi
{
    string fileExtension = ".drawio.svg";
    private string smName = "MySm";
    private string diagramFileName = ".drawio.svg";
    private string scriptFileName = "MySm.csx";
    private TargetLanguageId targetLanguageId = TargetLanguageId.CSharp;
    private string drawIoDiagramTemplateId = "drawio-simple-1"; // string to accomodate user templates someday
    private string plantUmlDiagramTemplateId = "advanced-1";    // string to accomodate user templates someday

    public void Run()
    {
        fileExtension = ".plantuml";

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
        // <remember>
        // yes
        // no

        // help setup vscode script intellisense?

        // # Confirm
        // State Machine Name: MySm
        // Target Language: C
        // StateSmith Version: 0.9.7-alpha
        // Diagram file: MySm.drawio.svg
        // Script file: MySm.csx
        // Template: basic
        // Generate markdown file: yes
        // hit enter to generate!


    }

    private void ScriptFileName()
    {
        AddSectionHeader("Script File Name/Path");
        AnsiConsole.MarkupLine("Script will be created in this directory unless you specify an absolute or relative path.");
        AnsiConsole.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        AnsiConsole.MarkupLine("");

        string defaultValue = smName + ".csx";
        scriptFileName = Ask("Enter script file name/path", defaultValue);

        scriptFileName = scriptFileName.Replace("$$", smName);
        AnsiConsole.MarkupLine($"Script file name/path: [blue]{scriptFileName}[/]");
    }

    private void DiagramFileName()
    {
        AddSectionHeader("Diagram File Name/Path");
        AnsiConsole.MarkupLine("Diagram will be created in this directory unless you specify an absolute or relative path.");
        AnsiConsole.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        AnsiConsole.MarkupLine("");

        string defaultValue = smName + fileExtension;
        diagramFileName = Ask("Enter diagram file name/path", defaultValue);

        diagramFileName = diagramFileName.Replace("$$", smName);
        AnsiConsole.MarkupLine($"Diagram file name/path: [blue]{diagramFileName}[/]");
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
        AddRememberedToChoices(choices, id:fileExtension);

        fileExtension = AnsiConsole.Prompt(
                new SelectionPrompt<Item<string>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        AnsiConsole.WriteLine($"Selected {fileExtension}");
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

        string versionChoice = Ask("Enter StateSmith version", defaultValue:"0.9.7-alpha");

        AnsiConsole.WriteLine($"Selected {versionChoice}");

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
        AskTemplate(choices, templateTypeName, ref drawIoDiagramTemplateId);
    }

    private void AskPlantUmlTemplate()
    {
        var choices = new List<Item<string>>()
        {
            new Item<string>(id: "plantuml-simple-1",    display: "" ),
        };
        var templateTypeName = "PlantUML";
        AskTemplate(choices, templateTypeName, ref plantUmlDiagramTemplateId);
    }

    private static void AskTemplate(List<Item<string>> choices, string templateTypeName, ref string templateSetting)
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
    }

    private bool IsDrawIoSelected()
    {
        return (fileExtension.Contains(".drawio"));
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

        AddRememberedToChoices(choices, id: targetLanguageId);

        targetLanguageId = AnsiConsole.Prompt(
                new SelectionPrompt<Item<TargetLanguageId>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        AnsiConsole.MarkupLine($"Selected [blue]{targetLanguageId}[/]");
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

        AnsiConsole.MarkupLine("This sets the name of the state machine in the diagram, generated code");
        AnsiConsole.MarkupLine("and will be used for auto suggesting file names in later steps.");
        AnsiConsole.MarkupLine("[grey]Default shown in (parenthesis).[/]");
        AnsiConsole.MarkupLine("");
        smName = Ask("Enter your state machine name", "MySm");
    }

    private void Updates()
    {
        AddSectionHeader("Updates");

        var checkForUpdates = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Check for updates?")
                    .AddChoices(new[] { "yes", "no" }));

        if (checkForUpdates == "yes")
        {
            CheckForUpdates();
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]Skipping updates...[/]");
        }
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
}
