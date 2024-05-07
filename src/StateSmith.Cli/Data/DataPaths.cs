using Spectre.Console;
using System;
using System.IO;

namespace StateSmith.Cli.Data;

public class DataPaths
{
    IAnsiConsole _console;
    public readonly string DataPath;

    public string Create_SettingsPersistencePath => $"{DataPath}/create-settings.json";
    public string Create_UpdateInfoPersistencePath => $"{DataPath}/create-update-info.json";
    public string Tool_UpdateInfo => $"{DataPath}/tool-update-info.json";
    public string Tool_Settings => $"{DataPath}/tool-settings.json";

    public DataPaths(IAnsiConsole console)
    {
        this._console = console;
        DataPath = GetOrMakeDataDirPath();
    }

    public string GetOrMakeDataDirPath()
    {
        string dirPath = "";

        Environment.SpecialFolder[] preferredFolders =
        {
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolder.Personal,
            Environment.SpecialFolder.UserProfile   // WSL2 on one of my computers returns blank for everything except for UserProfile
        };

        foreach (var specialFolder in preferredFolders)
        {
            dirPath = Environment.GetFolderPath(specialFolder);
            if (dirPath.Length > 0)
                break;
        }

        if (dirPath.Length == 0)
        {
            _console.MarkupLine("[red]Error![/] Couldn't find a directory to store settings! Using this directory.");
        }

        dirPath = Path.Combine(dirPath, "StateSmith.Cli");

        // alert user to us creating the settings directory
        if (!Directory.Exists(dirPath))
        {
            _console.MarkupLine($"[cyan]Creating settings directory: {dirPath}[/]");
            Directory.CreateDirectory(dirPath);
        }
        else
        {
            _console.MarkupLine($"[grey]Using settings directory: {dirPath}[/]");
        }

        return dirPath;
    }
}
