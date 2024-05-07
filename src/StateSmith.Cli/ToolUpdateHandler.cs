
using Spectre.Console;
using StateSmith.Cli.Data;
using StateSmith.Cli.Utils;
using System;

namespace StateSmith.Cli;

public class ToolUpdateHandler
{
    IAnsiConsole _console;
    DataPaths _settingsPaths;
    ToolSettings? toolSettings;
    JsonFilePersistence _persistence = new() { IncludeFields = false };

    public ToolUpdateHandler(IAnsiConsole console, DataPaths settingsPaths)
    {
        _console = console;
        _settingsPaths = settingsPaths;
    }

    public void AskIfUserWantsToCheckForUpdates()
    {
        try
        {
            toolSettings = _persistence.RestoreFromFile<ToolSettings>(_settingsPaths.Tool_UpdateSettings);
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error reading {nameof(Data)} file: {ex.Message}[/]. Using defaults.");
        }

        
    }
}
