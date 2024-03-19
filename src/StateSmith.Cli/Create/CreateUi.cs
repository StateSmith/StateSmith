using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using StateSmith.Cli.Utils;

namespace StateSmith.Cli.Create;

public class CreateUi
{
    internal Settings _settings = new();
    UpdateInfo _updatesInfo = new();
    private readonly string _settingsPersistencePath;
    private readonly string _updateInfoPersistencePath;
    string _latestStateSmithVersion = UpdateInfo.DefaultStateSmithLibVersion;
    IAnsiConsole _console = AnsiConsole.Console;

    public CreateUi()
    {
        string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!; //TODO consider null
        _settingsPersistencePath = $"{assemblyFolder}/ss-create-settings.json";
        _updateInfoPersistencePath = $"{assemblyFolder}/ss-create-update-info.json";
    }

    public void SetConsole(IAnsiConsole console)
    {
        _console = console;
    }

    public string GetSettingsPersistencePath() => _settingsPersistencePath;
    public string GetUpdateInfoPersistencePath() => _updateInfoPersistencePath;

    public void PrintPersistencePaths()
    {
        AddSectionHeader("Data/Settings Persistence Info");

        _console.MarkupLine($"Settings persistance path:\n  {GetSettingsPersistencePath()}\n");
        _console.MarkupLine($"Update Info persistance path:\n  {GetUpdateInfoPersistencePath()}\n");
    }

    public void Run()
    {
        ReadSettingsFromJson();

        Updates();

        StateSmithVersion();

        StateMachineName();

        AskTargetLanguage();

        DiagramType();

        DiagramFileName();

        ScriptFileName();

        DiagramTemplate();

        // TODO: ask if should generate state machine description markdown file (if version supports it)
        // TODOLOW - generate runnable example?

        SaveSettings();

        bool confirmation = AskConfirmation();
        if (!confirmation)
        {
            _console.MarkupLine("[red]Aborted.[/]");
            return;
        }

        confirmation = AskForOverwriteIfNeeded();
        if (!confirmation)
        {
            _console.MarkupLine("[red]Aborted.[/]");
            return;
        }

        new Generator(_settings).GenerateFiles();

        _console.MarkupLine($"[green]Success!.[/]");
    }

    private bool AskForOverwriteIfNeeded()
    {
        bool proceed = true;

        // if files exist, ask if should overwrite
        if (File.Exists(_settings.diagramFileName) || File.Exists(_settings.scriptFileName))
        {
            _console.MarkupLine("[red]Warning![/] Files already exist.");

            const string yesOverwriteStr = "yes, overwrite the existing files";
            bool overwrite = yesOverwriteStr == _console.Prompt(
                     new SelectionPrompt<string>()
                     .Title("Overwrite existing files?")
                     .AddChoices(new[] { "no", yesOverwriteStr }));

            if (overwrite)
            {
                _console.MarkupLine("[yellow]Overwriting existing files.[/]");
            }
            else
            {
                _console.MarkupLine("[red]Not overwriting files.[/]");
                proceed = false;
            }
        }

        return proceed;
    }

    private void ReadSettingsFromJson()
    {
        LoadVersionCheckInfoFromFile();
        LoadSettingsFromFile();
    }

    private void LoadSettingsFromFile()
    {
        //string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), settingsPath);
        //_console.MarkupLine($"Reading settings/data from path '{settingsPath}'.");

        if (!File.Exists(_settingsPersistencePath))
        {
            _console.MarkupLine($"[grey]Settings file not found. Using defaults.[/]");
            return;
        }

        try
        {
            _settings = JsonFilePersistence.RestoreFromFile<Settings>(_settingsPersistencePath);
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error reading {nameof(Settings)} file: {ex.Message}[/]. Using defaults.");
        }
    }

    private void LoadVersionCheckInfoFromFile()
    {
        //_console.MarkupLine($"Reading {nameof(UpdatesCheckInfo)} from path '{updatesCheckInfoPath}'.");

        if (!File.Exists(_updateInfoPersistencePath))
        {
            _console.MarkupLine($"[grey]{nameof(UpdateInfo)} file not found. Using defaults.[/]");
            return;
        }

        try
        {
            _updatesInfo = JsonFilePersistence.RestoreFromFile<UpdateInfo>(_updateInfoPersistencePath);
            _latestStateSmithVersion = _updatesInfo.LastestStateSmithLibStableVersion;
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error reading {nameof(UpdateInfo)} file: {ex.Message}[/]. Using defaults.");
        }
    }

    private bool AskConfirmation()
    {
        AddSectionHeader("Confirmation");
        _console.MarkupLine($"State Machine Name: [blue]{_settings.smName}[/]");
        _console.MarkupLine($"Target Language: [blue]{_settings.TargetLanguageId}[/]");
        _console.MarkupLine($"StateSmith Version: [blue]{_settings.StateSmithVersion}[/]");
        _console.MarkupLine($"Diagram file: [blue]{_settings.diagramFileName}[/]");
        _console.MarkupLine($"Script file: [blue]{_settings.scriptFileName}[/]");

        if (_settings.IsDrawIoSelected())
            _console.MarkupLine($"Template: [blue]{_settings.DrawIoDiagramTemplateId}[/]");
        else
            _console.MarkupLine($"Template: [blue]{_settings.PlantUmlDiagramTemplateId}[/]");

        _console.MarkupLine("");

        var confirmation = YesNoPrompt("Generate?");
        return confirmation;
    }

    internal void ScriptFileName()
    {
        AddSectionHeader("Script File Name/Path");
        _console.MarkupLine("Script will be created in this directory unless you specify an absolute or relative path.");
        _console.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        _console.MarkupLine("");

        string defaultValue = _settings.smName + ".csx";
        _settings.scriptFileName = Ask("Enter script file name/path", defaultValue);

        _settings.scriptFileName = _settings.scriptFileName.Replace("$$", _settings.smName);
        _console.MarkupLine($"Script file name/path: [blue]{_settings.scriptFileName}[/]");
    }

    internal void DiagramFileName()
    {
        AddSectionHeader("Diagram File Name/Path");
        _console.MarkupLine("Diagram will be created in this directory unless you specify an absolute or relative path.");
        _console.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        _console.MarkupLine("");

        string defaultValue = _settings.smName + _settings.FileExtension;
        _settings.diagramFileName = Ask("Enter diagram file name/path", defaultValue);

        _settings.diagramFileName = _settings.diagramFileName.Replace("$$", _settings.smName);
        _console.MarkupLine($"Diagram file name/path: [blue]{_settings.diagramFileName}[/]");
    }

    private void DiagramType()
    {
        AddSectionHeader("Diagram Type");
        _console.MarkupLine($"Choose the diagram file type you want to use.");
        _console.MarkupLine("[grey italic]More info: https://github.com/StateSmith/StateSmith/wiki/draw.io:-file-choice [/]");

        var choices = new List<UiItem<string>>()
        {
            new UiItem<string>(id: ".drawio.svg", display: "draw.io SVG [grey]\".drawio.svg\"[/]" ),
            new UiItem<string>(id: ".drawio",     display: "draw.io XML [grey]\".drawio\"[/]" ),
            new UiItem<string>(id: ".plantuml",   display: "PlantUML    [grey]\".plantuml\"[/]" ),
        };
        AddRememberedToChoices(choices, id: _settings.FileExtension);

        _settings.FileExtension = _console.Prompt(
                new SelectionPrompt<UiItem<string>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        _console.WriteLine($"Selected {_settings.FileExtension}");
    }

    private static bool AddRememberedToChoices<T>(List<UiItem<T>> choices, T id) where T : notnull
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

        string latestStableOption = $"latest stable available ([green]{_latestStateSmithVersion}[/])";
        string myLastUsedOption = $"my last used ([grey]{_settings.StateSmithVersion}[/])";
        const string customOption = "custom";

        string choice = _console.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which version of the StateSmith library do you want to use?")
                    .AddChoices(new[] { latestStableOption, myLastUsedOption, customOption }));

        if (choice == latestStableOption)
        {
            _settings.StateSmithVersion = _latestStateSmithVersion;
        }
        else if (choice == myLastUsedOption)
        {
            // do nothing. it's already set
        }
        else
        {
            _settings.StateSmithVersion = Ask("Enter StateSmith version", defaultValue: _latestStateSmithVersion);
            // TODOLOW validate version is valid
        }

        _console.WriteLine($"Selected {_settings.StateSmithVersion}");
    }

    private string Ask(string prompt, string defaultValue)
    {
        return _console.Prompt(
            new TextPrompt<string>(prompt)
                .DefaultValue(defaultValue)
                .DefaultValueStyle("grey")
                );
    }

    private void DiagramTemplate()
    {
        AddSectionHeader("Diagram Template");
        //_console.MarkupLine("[grey]?[/]");

        if (_settings.IsDrawIoSelected())
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
        var choices = new List<UiItem<string>>()
            {
                new UiItem<string>(id: TemplateIds.DrawIoSimple1,    display: "" ),
            };
        var templateTypeName = "draw.io";
        _settings.DrawIoDiagramTemplateId = AskTemplate(choices, templateTypeName, _settings.DrawIoDiagramTemplateId);
    }

    private void AskPlantUmlTemplate()
    {
        var choices = new List<UiItem<string>>()
        {
            new UiItem<string>(id: TemplateIds.PlantUmlSimple1,    display: "" ),
        };
        var templateTypeName = "PlantUML";
        _settings.PlantUmlDiagramTemplateId = AskTemplate(choices, templateTypeName, _settings.PlantUmlDiagramTemplateId);
    }

    private string AskTemplate(List<UiItem<string>> choices, string templateTypeName, string templateSetting)
    {
        bool found = AddRememberedToChoices(choices, id: templateSetting);

        if (!found)
        {
            _console.MarkupLine($"[bold yellow]Warning:[/] Remembered setting `[yellow]{templateSetting}[/]` not found.\n");
        }

        templateSetting = _console.Prompt(
                new SelectionPrompt<UiItem<string>>()
                    .Title($"Select {templateTypeName} template")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        _console.MarkupLine($"Selected [blue]{templateSetting}[/]");

        return templateSetting;
    }

    private void AskTargetLanguage()
    {
        AddSectionHeader("Target Language");

        var choices = new List<UiItem<TargetLanguageId>>()
        {
            new UiItem<TargetLanguageId>(id: TargetLanguageId.C,           display: "C99" ),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.CppC,        display: "C++ [grey](c style, improvements coming)[/]" ),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.CSharp,      display: "C#" ),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.JavaScript,  display: "JavaScript" ),
        };

        AddRememberedToChoices(choices, id: _settings.TargetLanguageId);

        _settings.TargetLanguageId = _console.Prompt(
                new SelectionPrompt<UiItem<TargetLanguageId>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        _console.MarkupLine($"Selected [blue]{_settings.TargetLanguageId}[/]");
    }

    private void AddSectionHeader(string header)
    {
        _console.MarkupLine("");

        var rule = new Rule($"[yellow]{header}[/]")
        {
            Justification = Justify.Left
        };
        _console.Write(rule);
    }

    private void StateMachineName()
    {
        AddSectionHeader("State Machine Name");

        _console.MarkupLine("This sets the name of the state machine in the diagram, generated code,");
        _console.MarkupLine("and will be used for auto suggesting file names in later steps.");
        _console.MarkupLine("[grey]Default shown in (parenthesis).[/]");
        _console.MarkupLine("");
        _settings.smName = Ask("Enter your state machine name", "MySm");
    }

    private void Updates()
    {
        AddSectionHeader("Updates");

        const string title = "Check for updates?";

        bool checkForUpdates;

        if (TimeToCheckForUpdates())
        {
            checkForUpdates = YesNoPrompt(title, yesText: "yes [grey](it has been a while since we last checked)[/]");
        }
        else
        {
            checkForUpdates = NoYesPrompt(title, noText: "no [grey](we just recently checked)[/]");
        }

        if (checkForUpdates)
        {
            CheckForUpdates();
        }
        else
        {
            _console.MarkupLine("[grey]Skipping updates...[/]");
        }
    }

    private bool TimeToCheckForUpdates()
    {
        const int RecommendedMsBetweenChecks = 1000 * 60 * 60 * 24;  // 1 day
        //const int RecommendedMsBetweenChecks = 1000 * 20; // for testing
        return _updatesInfo.GetMsSinceLastCheck() > RecommendedMsBetweenChecks;
    }

    private bool YesNoPrompt(string title, string yesText = "yes")
    {
        return yesText == _console.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(new[] { yesText, "no" }));
    }

    private bool NoYesPrompt(string title, string noText = "no")
    {
        const string yes = "yes";
        return yes == _console.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(new[] { noText, yes }));
    }

    private void CheckForUpdates()
    {
        //void WriteLogMessage(string message)
        //{
        //    _console.MarkupLine($"[grey]LOG:[/] {message}[grey]...[/]");
        //}

        NuGet.Versioning.NuGetVersion? latestStable = null;
        string? latestStableStr = null;

        // spinners in action: https://jsfiddle.net/sindresorhus/2eLtsbey/embedded/result/
        _console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start("[yellow]Checking for updates[/]", ctx =>
            {
                ctx.Status("checking for StateSmith library updates from nuget.org");

                try
                {
                    var task = NugetVersionGrabber.GetVersions("StateSmith");
                    var versions = task.Result;

                    // find latest stable version
                    latestStable = versions.Where(v => !v.IsLegacyVersion).OrderByDescending(v => v).First();
                    latestStableStr = latestStable.ToString();

                    _updatesInfo.LastCheckDateTime = DateTime.Now.ToString();
                }
                catch {
                }
            });

        if (latestStable == null || latestStableStr == null)
        {
            _console.MarkupLine("[red]Failed to get version info from nuget.org[/]");
            return;
        }

        bool newVersionFound = _updatesInfo.LastestStateSmithLibStableVersion != latestStableStr;
        _updatesInfo.LastestStateSmithLibStableVersion = latestStableStr;

        if (!newVersionFound)
        {
            _console.MarkupLine($"[grey]No new updates found since last check. The latest stable version is still {latestStable}.[/]");
        }
        else
        {
            _console.MarkupLine($"[green]New version found! {latestStable}.[/]");
        }

        _console.MarkupLine("");

        _latestStateSmithVersion = latestStableStr;

        //if (settings.StateSmithVersion != latestStableStr)
        //{
        //    bool update = YesNoPrompt($"Your last used (or default) is an old version ({settings.StateSmithVersion}). Do you want to use the latest stable?");
        //    if (update)
        //    {
        //        settings.StateSmithVersion = latestStableStr;
        //        SaveSettings();
        //    }
        //}
        //else
        //{
        //    //_console.MarkupLine($"You are already using the latest stable version of StateSmith: {latestStableStr}");
        //}

        JsonFilePersistence.PersistToFile(_updatesInfo, _updateInfoPersistencePath);
    }

    private void SaveSettings()
    {
        _console.MarkupLine("");
        _console.MarkupLine("[grey]Saving settings...[/]");

        JsonFilePersistence.PersistToFile(_settings, _settingsPersistencePath);

        _console.MarkupLine("[grey]Settings saved.[/]");
    }
}
