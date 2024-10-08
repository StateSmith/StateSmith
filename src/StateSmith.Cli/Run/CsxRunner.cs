using System.Collections.Generic;
using System.IO;
using System;
using StateSmith.Cli.Utils;
using StateSmith.Runner;

namespace StateSmith.Cli.Run;

public class CsxRunner
{
    private RunConsole _runConsole;
    private readonly string _searchDirectory;
    private readonly RunHandlerOptions _runHandlerOptions;

    private string CurrentDirectory => _runHandlerOptions.CurrentDirectory;
    private bool IsVerbose => _runHandlerOptions.Verbose;
    private bool IsNoCsx => _runHandlerOptions.NoCsx;
    private bool IsRebuild => _runHandlerOptions.Rebuild;

    public CsxRunner(RunConsole runConsole, RunInfo runInfo, string searchDirectory, RunHandlerOptions runHandlerOptions)
    {
        _runConsole = runConsole;
        _searchDirectory = searchDirectory;
        _runHandlerOptions = runHandlerOptions;
    }

    public void SetConsole(RunConsole runConsole)
    {
        _runConsole = runConsole;
    }

    public void RunScriptsIfNeeded(List<string> csxScripts, RunInfo runInfo, out bool ranFiles)
    {
        ranFiles = false;

        if (IsNoCsx)
        {
            _runConsole.QuietMarkupLine("Ignoring all .csx scripts for --no-csx.", filter: IsVerbose);
            return;
        }

        if (csxScripts.Count == 0)
        {
            _runConsole.MarkupLine("No .csx scripts found to run.");
            return;
        }

        (string? versionString, Exception? exception) = DotnetScriptProgram.TryGetVersionString();
        if (versionString == null)
        {
            _runConsole.ErrorMarkupLine($"Did not detect `{DotnetScriptProgram.Name}` program.");
            _runConsole.WarnMarkupLine($"Not attempting to run StateSmith .csx scripts:");
            foreach (var path in csxScripts)
            {
                _runConsole.WarnMarkupLine("    " + path);
            }
            _runConsole.WriteLine("");

            if (IsVerbose)
            {
                _runConsole.WriteLine("Attempted command to detect version:");
                _runConsole.WriteException(exception!);
                _runConsole.WriteLine("");
                _runConsole.MarkupLine("You can ignore .csx files with the [green]--no-csx[/] CLI option.");
            }

            return;
        }

        _runConsole.QuietMarkupLine($"Running StateSmith .csx scripts with `{DotnetScriptProgram.Name}` version: " + versionString, filter: IsVerbose);

        bool anyScriptsRan = false;

        foreach (var csxShortPath in csxScripts)
        {
            RunScriptIfNeeded(csxShortPath, runInfo, out bool scriptRan);
            anyScriptsRan |= scriptRan;
            //_console.WriteLine(); // already lots of newlines in RunScriptIfNeeded
        }

        if (!anyScriptsRan)
        {
            _runConsole.WriteLine("No scripts needed to be run.");
        }
        else
        {
            _runConsole.WriteLine("Finished running scripts.");
            ranFiles = true;
        }
    }

    public void RunScriptIfNeeded(string csxShortPath, RunInfo runInfo, out bool scriptRan, bool rebuildIfLastFailure = false)
    {
        string csxLongerPath = $"{_searchDirectory}/{csxShortPath}";
        string csxAbsolutePath = Path.GetFullPath(csxLongerPath);

        if (IsVerbose)
        {
            _runConsole.AddMildHeader($"Checking script and diagram dependencies for: `{csxShortPath}`");
        }

        var incrementalRunChecker = new IncrementalRunChecker(_runConsole, _searchDirectory, IsVerbose, runInfo);

        if (incrementalRunChecker.TestFilePath(csxAbsolutePath, rebuildIfLastFailure) != IncrementalRunChecker.Result.OkToSkip)
        {
            // already basically printed by IncrementalRunChecker
            //_console.WriteLine($"Script or its diagram dependencies have changed. Running script.");
        }
        else
        {
            if (IsRebuild)
            {
                _runConsole.MarkupLine("Would normally skip (file dates look good), but [yellow]rebuild[/] option set.", filter: IsVerbose);
            }
            else
            {
                _runConsole.QuietMarkupLine($"Script and its diagram dependencies haven't changed. Skipping.", filter: IsVerbose);
                scriptRan = false;
                return; //!!!!!!!!!!! NOTE the return here.
            }
        }

        _runConsole.WriteLine($"Running script: `{csxShortPath}`");
        scriptRan = true;

        SimpleProcess process = new()
        {
            WorkingDirectory = _searchDirectory,
            SpecificCommand = DotnetScriptProgram.Name,
            SpecificArgs = csxAbsolutePath,
            throwOnExitCode = false
        };
        process.EnableEchoToTerminal(_runConsole.GetIAnsiConsole());

        // Important that we grab time before running the process.
        // This ensures that we can detect if diagram or csx file was modified after our run.
        var info = new CsxRunInfo(csxAbsolutePath: csxAbsolutePath);
        runInfo.csxRuns[csxAbsolutePath] = info; // will overwrite if already exists
        process.Run(timeoutMs: 60000);
        info.lastCodeGenEndDateTime = DateTime.Now;

        // We need to parse output before potentially throwing to support Watch mode.
        CsxOutputParser parser = new();
        if (process.GetExitCode() != 0)
        {
            parser.Permissive();
        }
        parser.ParseAndResolveFilePaths(process.StdOutput, info);

        if (process.GetExitCode() != 0)
        {
            throw new FinishedWithFailureException();
        }

        info.success = true;
    }
}
