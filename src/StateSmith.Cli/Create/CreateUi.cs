using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using StateSmith.Cli.Utils;
using StateSmith.Cli.Data;
using System.Threading;

namespace StateSmith.Cli.Create;

public class CreateUi
{
    JsonFilePersistence _persistence = new() { IncludeFields = false };
    internal Settings _settings = new();
    UpdateInfo _updatesInfo = new();
    private readonly string _settingsPersistencePath;
    private readonly string _updateInfoPersistencePath;
    string _latestStateSmithVersion = UpdateInfo.DefaultStateSmithLibVersion;
    IAnsiConsole _console = AnsiConsole.Console;

    public CreateUi(IAnsiConsole console, DataPaths settingsPaths)
    {
        // printing in constructor because GetOrMakeConfigDirPath() prints.
        console.MarkupLine("");
        UiHelper.AddSectionLeftHeader(console, "Create");
        _settingsPersistencePath = settingsPaths.Create_SettingsPersistencePath;
        _updateInfoPersistencePath = settingsPaths.Create_UpdateInfoPersistencePath;
        this._console = console;
    }

    public string GetSettingsPersistencePath() => _settingsPersistencePath;
    public string GetUpdateInfoPersistencePath() => _updateInfoPersistencePath;

    public void Run()
    {
        ReadSettingsFromJson();

        SelectWorkflow();

        if (IsCsxWorkflow())
        {
            StateSmithLib();
        }

        StateMachineName();

        AskTargetLanguage();

        DiagramType();

        DiagramFileName();

        if (IsCsxWorkflow())
        {
            ScriptFileName();
        }

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

        _console.MarkupLine($"[green]Success![/]");
    }

    private bool AskForOverwriteIfNeeded()
    {
        bool proceed = true;

        // if files exist, ask if should overwrite
        if (File.Exists(_settings.diagramFileName) || WouldOverwiteScriptFile())
        {
            proceed = UiHelper.AskForOverwrite(_console);
        }

        return proceed;
    }

    private bool WouldOverwiteScriptFile()
    {
        if (!IsCsxWorkflow())
        {
            return false;
        }

        return File.Exists(_settings.scriptFileName);
    }

    private bool IsCsxWorkflow()
    {
        return _settings.UseCsxWorkflow;
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
            _settings = _persistence.RestoreFromFile<Settings>(_settingsPersistencePath);
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
            _updatesInfo = _persistence.RestoreFromFile<UpdateInfo>(_updateInfoPersistencePath);
            _latestStateSmithVersion = _updatesInfo.LatestStateSmithLibStableVersion;
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error reading {nameof(UpdateInfo)} file: {ex.Message}[/]. Using defaults.");
        }
    }

    private bool AskConfirmation()
    {
        AddSectionLeftHeader("Confirmation");
        _console.MarkupLine($"State Machine Name: [blue]{_settings.smName}[/]");
        _console.MarkupLine($"Target Language: [blue]{_settings.TargetLanguageId}[/]");
        
        if (IsCsxWorkflow())
        {
            _console.MarkupLine($"StateSmith Version: [blue]{_settings.StateSmithVersion}[/]");
        }

        _console.MarkupLine($"Diagram file: [blue]{_settings.diagramFileName}[/]");

        if (IsCsxWorkflow())
        {
            _console.MarkupLine($"Script file: [blue]{_settings.scriptFileName}[/]");
        }

        if (_settings.IsDrawIoSelected())
        {
            _console.MarkupLine($"Template: [blue]{_settings.DrawIoDiagramTemplateId}[/]");
        }
        else
        {
            _console.MarkupLine($"Template: [blue]{_settings.PlantUmlDiagramTemplateId}[/]");
        }

        _console.MarkupLine("");

        var confirmation = UiHelper.YesNoPrompt(_console, "Generate?");
        return confirmation;
    }

    internal void ScriptFileName()
    {
        AddSectionLeftHeader("Script File Name/Path");
        _console.MarkupLine("Script will be created in this directory unless you specify an absolute or relative path.");
        _console.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        _console.MarkupLine("");

        string defaultValue = _settings.smName + ".csx";
        _settings.scriptFileName = Ask("Enter script file name/path", defaultValue);

        _settings.scriptFileName = _settings.scriptFileName.Replace("$$", defaultValue);
        _console.MarkupLine($"Script file name/path: [blue]{_settings.scriptFileName}[/]");
    }

    internal void DiagramFileName()
    {
        AddSectionLeftHeader("Diagram File Name/Path");
        _console.MarkupLine("Diagram will be created in this directory unless you specify an absolute or relative path.");
        _console.MarkupLine("[grey]You can use \"$$\" as the suggested filename if you want to specify a path.[/] [grey italic]ex: ../../$$ [/]");
        _console.MarkupLine("");

        string defaultValue = _settings.smName + _settings.FileExtension;
        _settings.diagramFileName = Ask("Enter diagram file name/path", defaultValue);

        _settings.diagramFileName = _settings.diagramFileName.Replace("$$", defaultValue);
        _console.MarkupLine($"Diagram file name/path: [blue]{_settings.diagramFileName}[/]");
    }

    private void DiagramType()
    {
        AddSectionLeftHeader("Diagram Type");
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

    private void SelectWorkflow()
    {
        AddSectionLeftHeader("Workflow");
        _console.WriteLine("Please select your ideal workflow. We recommend starting with \"User Friendly\".");
        _console.WriteLine("You can manually add a .csx file later if you need more advanced features.");

        bool idForUseCsxWorkflow = true;
        bool idForUserFriendly = !idForUseCsxWorkflow;
        var choices = new List<UiItem<bool>>()
        {
            new UiItem<bool>(id: idForUserFriendly,    display: "User Friendly - generally recommended. Faster & simpler." ),
            new UiItem<bool>(id: idForUseCsxWorkflow,  display: "Advanced - adds a .csx file to support advanced features." ),
        };
        AddRememberedToChoices(choices, id: IsCsxWorkflow());

        _settings.UseCsxWorkflow = _console.Prompt(
                new SelectionPrompt<UiItem<bool>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        string selectedMessage = IsCsxWorkflow() ? "Advanced Features" : "User Friendly";
        _console.MarkupLine($"Selected [blue]{selectedMessage}[/]");
    }

    private void StateSmithVersion()
    {
        //AddSectionLeftHeader("StateSmith Version");
        _console.WriteLine("");

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
        AddSectionLeftHeader("Diagram Template");
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
                new UiItem<string>(id: TemplateIds.DrawIoPages1,  display: "Multiple pages - new features. REQUIRES lib version 0.12.0+."),
                new UiItem<string>(id: TemplateIds.DrawIoSimple1, display: "Traditional - single page."),
            };
        var templateTypeName = "draw.io";
        _settings.DrawIoDiagramTemplateId = AskTemplate(choices, templateTypeName, _settings.DrawIoDiagramTemplateId);
    }

    private void AskPlantUmlTemplate()
    {
        var choices = new List<UiItem<string>>()
        {
            new UiItem<string>(id: TemplateIds.PlantUmlMinimal1, display: "Minimal 1 - working example with `count` var." ),
            new UiItem<string>(id: TemplateIds.PlantUmlSimple1,  display: "Simple  1 - color style, composite state, function calls." ),
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
        AddSectionLeftHeader("Target Language");

        var choices = new List<UiItem<TargetLanguageId>>()
        {
            new UiItem<TargetLanguageId>(id: TargetLanguageId.C,           display: "C99"),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.CppC,        display: "C++ [grey](c style, improvements coming)[/]"),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.CSharp,      display: "C#"),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.JavaScript,  display: "JavaScript"),
            new UiItem<TargetLanguageId>(id: TargetLanguageId.Java,        display: "Java"),
        };

        AddRememberedToChoices(choices, id: _settings.TargetLanguageId);

        _settings.TargetLanguageId = _console.Prompt(
                new SelectionPrompt<UiItem<TargetLanguageId>>()
                    .Title("")
                    .UseConverter(x => x.Display)
                    .AddChoices(choices)).Id;

        _console.MarkupLine($"Selected [blue]{_settings.TargetLanguageId}[/]");
    }

    private void AddSectionLeftHeader(string header)
    {
        UiHelper.AddSectionLeftHeader(_console, header);
    }

    private void StateMachineName()
    {
        AddSectionLeftHeader("State Machine Name");

        _console.MarkupLine("This sets the name of the state machine in the diagram, generated code,");
        _console.MarkupLine("and will be used for auto suggesting file names in later steps.");
        _console.MarkupLine("[grey]Default shown in (parenthesis).[/]");
        _console.MarkupLine("");
        _settings.smName = Ask("Enter your state machine name", "MySm");
    }

    private void StateSmithLib()
    {
        AddSectionLeftHeader("StateSmith Library");

        const string title = "Check for a new version of StateSmith library used for code gen?";

        bool checkForUpdates;

        if (TimeToCheckForUpdates())
        {
            checkForUpdates = UiHelper.YesNoPrompt(_console, title, yesText: "yes [grey](it has been a while since we last checked)[/]");
        }
        else
        {
            checkForUpdates = UiHelper.NoYesPrompt(_console, title, noText: "no [grey](we just recently checked)[/]");
        }

        if (checkForUpdates)
        {
            CheckForUpdates();
        }
        else
        {
            _console.MarkupLine("[grey]Skipping check for updated lib.[/]");
        }

        Thread.Sleep(500); // give user a chance to read the last message

        StateSmithVersion();
    }

    private bool TimeToCheckForUpdates()
    {
        const int RecommendedMsBetweenChecks = 1000 * 60 * 60 * 24;  // 1 day
        //const int RecommendedMsBetweenChecks = 1000 * 20; // for testing
        return _updatesInfo.GetMsSinceLastCheck() > RecommendedMsBetweenChecks;
    }

    private void CheckForUpdates()
    {
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

        bool newVersionFound = _updatesInfo.LatestStateSmithLibStableVersion != latestStableStr;
        _updatesInfo.LatestStateSmithLibStableVersion = latestStableStr;

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

        _persistence.PersistToFile(_updatesInfo, _updateInfoPersistencePath);
    }

    private void SaveSettings()
    {
        _console.MarkupLine("");
        _console.MarkupLine("[grey]Saving settings...[/]");

        _persistence.PersistToFile(_settings, _settingsPersistencePath);

        _console.MarkupLine("[grey]Settings saved.[/]");
    }
}
