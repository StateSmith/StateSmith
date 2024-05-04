using Spectre.Console;
using StateSmith.Cli.Utils;
using System;
using System.IO;
using System.Threading;

namespace StateSmith.Cli.Setup;

public class SetupVscodeCsxAction
{
    private IAnsiConsole _console;

    public SetupVscodeCsxAction(IAnsiConsole console)
    {
        _console = console;
    }

    public void NotifyIfDotnetScriptOld()
    {
        UiHelper.AddSectionLeftHeader(_console, "Checking if `dotnet-script` is up to date");

        SimpleProcess process = new()
        {
            SpecificCommand = "dotnet-script",
            SpecificArgs = "--version",
            throwOnExitCode = true
        };
        process.Run(timeoutMs: 3000);

        // parse version into major, minor, patch
        string versionStr = process.StdOutputBuf.ToString().Trim();
        var version = new Version(versionStr);

        if (version.Major <= 1 && version.Minor <= 4) {
            _console.MarkupLine($"[cyan]Your `dotnet-script` version '{versionStr}' is old (1.5.0 exists).[/]");
            _console.Markup("You can update it by running: ");
            _console.MarkupLine("[cyan]dotnet tool update -g dotnet-script[/]");

            Thread.Sleep(1000); // give time for user to read
        }
        else
        {
            _console.MarkupLine("[green]✓ version looks good.[/]");
        }

        _console.WriteLine();
    }

    public void Run()
    {
        NotifyIfDotnetScriptOld();

        UiHelper.AddSectionLeftHeader(_console, "Setup vscode for C# script debugging");

        if (!File.Exists("launch.json"))
        {
            AddNewLaunchJson();
        }
        else
        {
            ModifyExistingLaunchJsonFile();
        }

        UiHelper.AddSectionLeftHeader(_console, "Setup complete", "green");
        _console.MarkupLine("Tip: you may want to git ignore the [yellow]omnisharp.json[/] file.");
        _console.Markup("Tip: more .csx related information: ");
        _console.MarkupLine("[blue][u]https://github.com/StateSmith/StateSmith/wiki/csx[/][/]");
    }

    private void ModifyExistingLaunchJsonFile()
    {
        const string dummyCsxName = "delete_me_dummy_file.csx";

        _console.MarkupLine("Modifying existing `launch.json` file for debugging .csx files in vscode.");
        _console.MarkupLine($"Running command [yellow]dotnet-script init {dummyCsxName}[/]:");

        SimpleProcess process = new()
        {
            WorkingDirectory = Environment.CurrentDirectory,
            SpecificCommand = "dotnet-script",
            SpecificArgs = $"init {dummyCsxName}",
            throwOnExitCode = true
        };
        process.EnableEchoToTerminal();
        process.Run(timeoutMs: 3000);

        // delete dummy file if it exists
        if (File.Exists(dummyCsxName))
        {
            _console.MarkupLine("Deleting dummy .csx file from above command.");
            File.Delete(dummyCsxName);
        }

        // TODO: check if generated launch file has absolute paths that should be fixed
        // read launch.json and check if it has absolute paths

        var launchJson = File.ReadAllText(".vscode/launch.json");

        string? newLaunchJson = LaunchJsonModder.MaybeMod(launchJson);
        if (newLaunchJson != null)
        {
            File.WriteAllText(".vscode/launch.json", newLaunchJson);
        }
    }

    private void AddNewLaunchJson()
    {
        _console.MarkupLine("Creating a brand new `launch.json` file for debugging .csx files in VSCode.");

        const string template = @"
{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""debug StateSmith code gen"",
            ""type"": ""coreclr"",
            ""request"": ""launch"",
            ""program"": ""${env:HOME}/.dotnet/tools/dotnet-script"",
            ""args"": [
                ""${file}""
            ],
            ""windows"": {
                ""program"": ""${env:USERPROFILE}/.dotnet/tools/dotnet-script.exe"",
            },
            ""logging"": {
                ""moduleLoad"": false
            },
            ""cwd"": ""${workspaceRoot}"",
            ""stopAtEntry"": false
        },
    ]
}";

        File.WriteAllText("launch.json", template);
    }
}
