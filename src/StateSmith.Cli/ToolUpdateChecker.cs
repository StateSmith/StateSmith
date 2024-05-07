
using Spectre.Console;
using StateSmith.Cli.Data;
using StateSmith.Cli.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace StateSmith.Cli;

public class ToolUpdateChecker
{
    readonly TrackingConsole _console;
    readonly string _dataPath;
    readonly JsonFilePersistence _persistence = new() { IncludeFields = false };

    public bool Printed => _console.Accessed;

    public ToolUpdateChecker(IAnsiConsole console, DataPaths settingsPaths)
    {
        _console = new TrackingConsole(console);
        _dataPath = settingsPaths.Tool_UpdateInfo;
    }

    public void AskToCheckIfTime(ToolSettings toolSettings)
    {
        if (toolSettings.SecondsBetweenUpdateChecks < 0)
        {
            return;
        }

        if (IsTimeToCheck(toolSettings))
        {
            UiHelper.AddSectionLeftHeader(_console.Get(), "Tool Update Check");
            if (UiHelper.YesNoPrompt(_console.Get(), "Check for tool updates? (It's been a while)"))
            {
                CheckForUpdates();
            }
            else
            {
                _console.Get().MarkupLine("[grey]Skipping update check. Will remind again next period.[/]");
                SaveCheckTime();
            }
        }
    }

    private void SaveCheckTime()
    {
        SaveToFile(ToolUpdateInfo.CreateForNow());
    }

    private void SaveToFile(ToolUpdateInfo toolUpdateInfo)
    {
        _persistence.PersistToFile(toolUpdateInfo, _dataPath);
    }

    private bool IsTimeToCheck(ToolSettings toolSettings)
    {
        bool timeToCheck = true;

        // try loading the last time we checked for updates
        try
        {
            if (File.Exists(_dataPath))
            {
                var updateInfo = _persistence.RestoreFromFile<ToolUpdateInfo>(_dataPath);
                if (updateInfo.GetSecondsSinceLastCheck() < toolSettings.SecondsBetweenUpdateChecks)
                {
                    timeToCheck = false;
                }
            }
        }
        catch (Exception ex)
        {
            _console.Get().MarkupLine($"[red]Error loading last update check time.[/]");
            _console.Get().WriteException(ex);
        }

        return timeToCheck;
    }

    public void CheckForUpdates(bool pauseForKeyboardEnter = true)
    {
        NuGet.Versioning.NuGetVersion? latestStable = null;
        string? latestStableStr = null;

        // spinners in action: https://jsfiddle.net/sindresorhus/2eLtsbey/embedded/result/
        _console.Get().Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start("[yellow]Checking for updates[/]", ctx =>
            {
                ctx.Status("checking for `StateSmith.Cli` updates from nuget.org");

                try
                {
                    var task = NugetVersionGrabber.GetVersions("StateSmith.Cli");
                    var versions = task.Result;

                    // find latest stable version
                    latestStable = versions.Where(v => !v.IsLegacyVersion).OrderByDescending(v => v).First();
                    latestStableStr = latestStable.ToString();
                }
                catch
                {
                }
            });

        if (latestStable == null || latestStableStr == null)
        {
            _console.Get().MarkupLine("[red]Failed to get version info from nuget.org[/]");
            return;
        }

        SaveCheckTime();

        // get this tool's version
        var thisVersion = typeof(ToolUpdateChecker).Assembly.GetName().Version;

        // compare versions
        if (thisVersion < latestStable.Version)
        {
            _console.Get().MarkupLine($"[cyan]!!! Good news !!![/]");
            _console.Get().MarkupLine($"A new version of StateSmith.Cli is available: [cyan]{latestStableStr}[/]");
            _console.Get().MarkupLine($"Install with this command: [cyan]dotnet tool update -g StateSmith.Cli[/]");
            _console.Get().MarkupLine("Change log: [blue][u]https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/CHANGELOG.md[/][/]");

            if (pauseForKeyboardEnter)
            {
                _console.Get().MarkupLine("\n");

                Thread.Sleep(1000);
                _console.Get().MarkupLine("Press enter to continue...");
                Console.ReadLine();
            }
        }
        else
        {
            _console.Get().MarkupLine("You are using the latest version of StateSmith.Cli");
            Thread.Sleep(1000);
        }
    }
}
