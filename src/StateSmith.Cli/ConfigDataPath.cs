using Spectre.Console;
using System;
using System.IO;

namespace StateSmith.Cli;

public class ConfigDataPath
{
    public string GetOrMakeConfigDirPath(Spectre.Console.IAnsiConsole console)
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
            console.MarkupLine("[red]Error![/] Couldn't find a directory to store settings! Using this directory.");
        }

        dirPath = Path.Combine(dirPath, "StateSmith.Cli");

        // alert user to us creating the settings directory
        if (!Directory.Exists(dirPath))
        {
            console.MarkupLine($"[cyan]Creating settings directory: {dirPath}[/]");
            Directory.CreateDirectory(dirPath);
        }
        else
        {
            console.MarkupLine($"[grey]Using settings directory: {dirPath}[/]");
        }

        return dirPath;
    }
}
