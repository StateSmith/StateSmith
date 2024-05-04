using Spectre.Console;
using StateSmith.Cli.Utils;
using System;
using System.IO;
using System.Threading;

namespace StateSmith.Cli.Setup;

public class SetupVscodeCsxAction
{
    public const string Description = "Set up vscode for C# script debugging and intellisense.";

    private IAnsiConsole _console;
    private const string vscodeWorkspaceSettingsPath = ".vscode";
    private const string launchJsonPath = vscodeWorkspaceSettingsPath + "/launch.json";

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
        Directory.CreateDirectory(vscodeWorkspaceSettingsPath); // ensure .vscode folder exists

        NotifyIfDotnetScriptOld();

        UiHelper.AddSectionLeftHeader(_console, Description);

        if (!File.Exists(launchJsonPath))
        {
            AddNewLaunchJson();
        }
        else
        {
            AutoModExistingLaunchJson();

            _console.MarkupLine($"[cyan]WARN![/] [yellow]{launchJsonPath}[/] already exists. You should manually add this config:");
            _console.MarkupLine("[blue][u]https://github.com/StateSmith/StateSmith/wiki/vscode-csx-launch.json-file[/][/]");
            _console.WriteLine();
            Thread.Sleep(1000); // give time for user to read
        }

        CreateOmnisharpFileIfNeeded();

        UiHelper.AddSectionLeftHeader(_console, "Setup complete", "green");
        _console.MarkupLine("Tip! You may want to git ignore the [yellow]omnisharp.json[/] file.");
        _console.MarkupLine("Tip! Useful vscode command: [yellow]OmniSharp: Restart OmniSharp[/]");
        _console.MarkupLine("Tip! Useful vscode command: [yellow]OmniSharp: Select Project[/]");
        _console.MarkupLine("Tip! More .csx related information: ");
        _console.MarkupLine("[blue][u]https://github.com/StateSmith/StateSmith/wiki/csx[/][/]");
    }

    private static void AutoModExistingLaunchJson()
    {
        // todolow - automatically add config to existing launch.json

        // Below code assumed that dotnet-script init would add the config, but it doesn't.
        // It needs some adjustments to add to existing launch.json
        bool supported = false;
        if (supported)
        {
            var launchJson = File.ReadAllText(launchJsonPath);

            string? newLaunchJson = LaunchJsonModder.MaybeMod(launchJson);
            if (newLaunchJson != null)
            {
                File.WriteAllText(launchJsonPath, newLaunchJson);
            }
        }
    }

    private void CreateOmnisharpFileIfNeeded()
    {
        if (File.Exists("omnisharp.json"))
        {
            _console.MarkupLine("[grey]omnisharp.json already exists (skipping).[/]");
            return;
        }

        const string dummyCsxName = "delete_me_dummy_file.csx";

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
    }

    private void AddNewLaunchJson()
    {
        _console.MarkupLine("Creating a brand new `launch.json` file for debugging .csx files in VSCode.");

        const string template = 
@"{
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

        File.WriteAllText(launchJsonPath, template);
    }
}
