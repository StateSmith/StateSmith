using Spectre.Console;
using StateSmith.Cli.Utils;
using System;
using System.IO;
using System.Threading;

namespace StateSmith.Cli.Setup;

public class SetupVscodeCsxAction
{
    public const string Description = "vscode for C# script debugging and intellisense.";

    private bool _verbose;
    private IAnsiConsole _console;
    private readonly string _currentDirectory;
    private const string vscodeWorkspaceSettingsPath = ".vscode";
    private const string launchJsonPath = vscodeWorkspaceSettingsPath + "/launch.json";

    public SetupVscodeCsxAction(IAnsiConsole console, bool verbose, string currentDirectory)
    {
        _console = console;
        _verbose = verbose;
        _currentDirectory = currentDirectory;
    }

    public void NotifyIfDotnetScriptOld(out bool dotnetScriptDetected)
    {
        UiHelper.AddSectionLeftHeader(_console, $"Checking if `{DotnetScriptProgram.Name}` is up to date");

        (string? versionStr, Exception? e) = DotnetScriptProgram.TryGetVersionString();
        if (versionStr == null)
        {
            _console.MarkupLine($"[red]Could not determine `{DotnetScriptProgram.Name}` version.[/]");
            _console.MarkupLine($"Try running `{DotnetScriptProgram.Name} --version` in your terminal.");

            if (_verbose)
            {
                _console.WriteLine();
                _console.WriteException(e!);
                _console.WriteLine();
            }

            dotnetScriptDetected = false;
            return;
        }

        var version = new Version(versionStr);

        if (version.Major <= 1 && version.Minor <= 4)
        {
            _console.MarkupLine($"[cyan]Your `{DotnetScriptProgram.Name}` version '{versionStr}' is old (1.5.0 exists).[/]");
            _console.Markup("You can update it by running: ");
            _console.MarkupLine($"[cyan]dotnet tool update -g {DotnetScriptProgram.Name}[/]");

            Thread.Sleep(1000); // give time for user to read
        }
        else
        {
            _console.MarkupLine("[green]✓ version looks good.[/]");
        }

        _console.WriteLine();
        dotnetScriptDetected = true;
    }

    public void Run()
    {
        Directory.CreateDirectory(vscodeWorkspaceSettingsPath); // ensure .vscode folder exists

        NotifyIfDotnetScriptOld(out bool dotnetScriptDetected);
        if (!dotnetScriptDetected)
        {
            _console.MarkupLine($"[red]Aborting vscode setup for .csx files.[/] Requires program `{DotnetScriptProgram.Name}`.");
            return;
        }

        UiHelper.AddSectionLeftHeader(_console, "Set up " + Description);

        CreateLaunchJsonIfNeeded();
        RunDotnetScriptInit();

        UiHelper.AddSectionLeftHeader(_console, "vscode csx setup complete", "green");
        _console.MarkupLine("Tip! You may want to git ignore the [yellow]omnisharp.json[/] file.");
        _console.MarkupLine("Tip! Useful vscode command: [yellow]OmniSharp: Restart OmniSharp[/]");
        _console.MarkupLine("Tip! Useful vscode command: [yellow]OmniSharp: Select Project[/]");
        _console.MarkupLine("Tip! [cyan]READ THIS[/] install/setup info: ");
        _console.MarkupLine("[blue][u]https://github.com/StateSmith/StateSmith/wiki/vscode-csx[/][/]");
    }

    private void CreateLaunchJsonIfNeeded()
    {
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

    /// <summary>
    /// Workaround for dotnet-script init overwriting launch.json
    /// See https://github.com/StateSmith/StateSmith/issues/263
    /// </summary>
    private void RunDotnetScriptInit()
    {
        string? oldLaunchJson = null;

        if (File.Exists(launchJsonPath))
        {
            oldLaunchJson = File.ReadAllText(launchJsonPath);
        }

        RunDotnetScriptInitThatOverwiteLaunch();

        if (oldLaunchJson != null)
        {
            File.WriteAllText(launchJsonPath, oldLaunchJson);
        }
    }

    /// <summary>
    /// Note! will overwrite existing launch.json file.
    /// https://github.com/StateSmith/StateSmith/issues/263
    /// </summary>
    private void RunDotnetScriptInitThatOverwiteLaunch()
    {
        if (File.Exists("omnisharp.json"))
        {
            _console.MarkupLine("[grey]omnisharp.json already exists (skipping).[/]");
            return;
        }

        const string dummyCsxName = "delete_me_dummy_file.csx";

        _console.MarkupLine($"Running command [yellow]{DotnetScriptProgram.Name} init {dummyCsxName}[/]:");

        SimpleProcess process = new()
        {
            WorkingDirectory = _currentDirectory,
            SpecificCommand = DotnetScriptProgram.Name,
            SpecificArgs = $"init {dummyCsxName}",
            throwOnExitCode = true
        };
        process.EnableEchoToTerminal();
        process.Run(timeoutMs: 20000);

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
