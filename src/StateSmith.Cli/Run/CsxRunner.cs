using StateSmith.Runner;
using System.Collections.Generic;
using System.IO;
using System;
using StateSmith.Cli.Utils;

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

    public void RunScriptsIfNeeded(List<string> csxScripts, RunInfo runInfo, out bool ranFiles)
    {
        ranFiles = false;

        if (IsNoCsx)
        {
            if (IsVerbose)
            {
                _runConsole.QuietMarkupLine("Ignoring all .csx scripts for --no-csx.");
            }
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

        if (IsVerbose)
        {
            _runConsole.QuietMarkupLine($"Running StateSmith .csx scripts with `{DotnetScriptProgram.Name}` version: " + versionString);
        }

        bool anyScriptsRan = false;

        foreach (var csxShortPath in csxScripts)
        {
            anyScriptsRan |= RunScriptIfNeeded(searchDirectory: _searchDirectory, csxShortPath, runInfo);
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

    private bool RunScriptIfNeeded(string searchDirectory, string csxShortPath, RunInfo runInfo)
    {
        bool scriptRan = false;
        string csxLongerPath = $"{searchDirectory}/{csxShortPath}";
        string csxAbsolutePath = Path.GetFullPath(csxLongerPath);

        _runConsole.AddMildHeader($"Checking script and diagram dependencies for: `{csxShortPath}`");

        var incrementalRunChecker = new IncrementalRunChecker(_runConsole.GetIAnsiConsole(), _searchDirectory, IsVerbose, runInfo);

        if (incrementalRunChecker.TestFilePath(csxAbsolutePath) != IncrementalRunChecker.Result.OkToSkip)
        {
            // already basically printed by IncrementalRunChecker
            //_console.WriteLine($"Script or its diagram dependencies have changed. Running script.");
        }
        else
        {
            if (IsRebuild)
            {
                _runConsole.MarkupLine("Would normally skip (file dates look good), but [yellow]rebuild[/] option set.");
            }
            else
            {
                _runConsole.QuietMarkupLine($"Script and its diagram dependencies haven't changed. Skipping.");
                return scriptRan; //!!!!!!!!!!! NOTE the return here.
            }
        }

        _runConsole.WriteLine($"Running script: `{csxShortPath}`");
        scriptRan = true;

        SimpleProcess process = new()
        {
            WorkingDirectory = searchDirectory,
            SpecificCommand = DotnetScriptProgram.Name,
            SpecificArgs = csxAbsolutePath,
            throwOnExitCode = false
        };
        process.EnableEchoToTerminal(_runConsole.GetIAnsiConsole());

        // Important that we grab time before running the process.
        // This ensures that we can detect if diagram or csx file was modified after our run.
        var info = new CsxRunInfo(csxAbsolutePath: csxAbsolutePath);
        process.Run(timeoutMs: 60000);
        info.lastCodeGenEndDateTime = DateTime.Now;

        if (process.GetExitCode() != 0)
        {
            throw new FinishedWithFailureException();
        }

        CsxOutputParser parser = new();
        parser.ParseAndResolveFilePaths(process.StdOutput, info);
        runInfo.csxRuns[csxAbsolutePath] = info; // will overwrite if already exists
        return scriptRan;
    }
}
