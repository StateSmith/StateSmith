
using Spectre.Console;
using StateSmith.Cli.Data;
using StateSmith.Cli.Utils;
using StateSmith.Common;
using System;
using System.IO;
using System.Linq;

namespace StateSmith.Cli;

public class ToolSettingsLoader
{
    readonly TrackingConsole _console;
    readonly string _dataPath;
    readonly JsonFilePersistence _persistence = new() { IncludeFields = false };
    
    ToolSettings? toolSettings;

    public bool Printed => _console.Accessed;
    
    public ToolSettings GetToolSettings() => toolSettings.ThrowIfNull();

    public ToolSettingsLoader(IAnsiConsole console, DataPaths settingsPaths)
    {
        _console = new TrackingConsole(console);
        _dataPath = settingsPaths.Tool_Settings;
    }

    public void LoadOrAskUser()
    {
        try
        {
            if (File.Exists(_dataPath))
            {
                LoadExistingSettings();
            }
            else
            {
                FirstRunDetected();
                SaveToFile();
            }
        }
        catch (Exception ex)
        {
            _console.Get().MarkupLine($"[red]Error loading tool settings. Using defaults.[/]");
            _console.Get().WriteException(ex);
            toolSettings = new ToolSettings();
            SaveToFile();
        }
    }

    private void SaveToFile()
    {
        _persistence.PersistToFile(toolSettings, _dataPath);
    }

    /// <summary>
    /// Throws if bad data.
    /// </summary>
    private void LoadExistingSettings()
    {
        try
        {
            toolSettings = _persistence.RestoreFromFile<ToolSettings>(_dataPath);
        }
        catch (Exception)
        {
            _console.Get().MarkupLine($"[red]Error reading {_dataPath}.[/]");
            throw;
        }
    }

    private void FirstRunDetected()
    {
        toolSettings = new ToolSettings();

        UiHelper.AddSectionLeftHeader(_console.Get(), "First Run Detected", "green");
        _console.Get().MarkupLine("Welcome! Let's set up your tool settings.\n");

        var choices = new[]
        {
            new UiItem<double>(TimeSpan.FromHours(1).TotalSeconds, "Hourly"),
            new UiItem<double>(TimeSpan.FromDays(1).TotalSeconds, "Daily"),
            new UiItem<double>(TimeSpan.FromDays(7).TotalSeconds, "Weekly"),
            new UiItem<double>(TimeSpan.FromDays(30).TotalSeconds, "Monthly"),
            new UiItem<double>(-1, "Never"),
        };

        toolSettings.SecondsBetweenUpdateChecks = UiHelper.Prompt(_console.Get(), "How often would you like a prompt to auto check for updates?", choices);
    }
}
