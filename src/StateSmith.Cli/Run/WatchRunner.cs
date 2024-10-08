using StateSmith.Cli.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace StateSmith.Cli.Run;

public class WatchRunner
{
    private RunConsole _console;
    private CsxRunner _csxRunner;
    private DiagramRunner _diagramRunner;
    private readonly RunInfoStore _runInfoStore;
    private List<string> _csxFilesToRun = new();
    private List<string> _diagramFilesToRun = new();
    private readonly RunHandlerOptions _runHandlerOptions;
    private readonly RunInfoDataBase _runInfoDataBase;
    private readonly string _searchDirectory;

    private SuccessTracker _successTracker = new();

    private bool IsVerbose => _runHandlerOptions.Verbose;
    private bool IsNoCsx => _runHandlerOptions.NoCsx;

    public WatchRunner(RunConsole console, CsxRunner csxRunner, DiagramRunner diagramRunner, RunInfoStore runInfoStore, RunHandlerOptions runHandlerOptions, RunInfoDataBase runInfoDataBase, string searchDirectory)
    {
        _console = console;
        //var silentConsole = console.CloneWithoutSettings();
        //silentConsole.SetSilent(true);

        _csxRunner = csxRunner;
        //_csxRunner.SetConsole(silentConsole);
        _diagramRunner = diagramRunner;

        _runInfoStore = runInfoStore;
        _runHandlerOptions = runHandlerOptions;
        _runInfoDataBase = runInfoDataBase;
        _searchDirectory = searchDirectory;
    }

    public void Run(SsCsxDiagramFileFinder.ScanResults scanResults)
    {
        _runHandlerOptions.Rebuild = false; // we don't want constant builds

        if (!IsNoCsx)
        {
            _csxFilesToRun.AddRange(scanResults.targetCsxFiles);
        }
        else
        {
            if (IsVerbose)
            {
                _console.QuietMarkupLine("Ignoring all .csx scripts for --no-csx.");
            }
        }

        _diagramFilesToRun.AddRange(scanResults.targetDiagramFiles);

        RunInner();
    }

    private void RunInner()
    {
        bool someFilesToRun = false;

        if (_diagramFilesToRun.Count > 0)
        {
            someFilesToRun = true;

            _console.MarkupLine("Watching the following diagram files:");
            foreach (var diagramFile in _diagramFilesToRun)
            {
                _console.MarkupLine($"- {diagramFile}");
            }
        }

        SetupAndPrintCsxFiles();
        someFilesToRun |= _csxFilesToRun.Count > 0;

        if (!someFilesToRun)
        {
            _console.MarkupLine("No files to watch.");
        }
        else
        {
            Stopwatch messageStopwatch = new();
            messageStopwatch.Start();
            UiHelper.AddSectionLeftHeader(_console.GetIAnsiConsole(), "Watching Files");
            bool firstRun = true;

            while (true)
            {
                WatchIterate(messageStopwatch, firstRun);
                firstRun = false;
                Thread.Sleep(1000);
            }
        }
    }

    private void WatchIterate(Stopwatch messageStopwatch, bool firstRun)
    {
        bool ranFiles = false;

        // csx files need to be run first so that we know which diagram files they use
        ranFiles |= RunCsxFilesIfNeeded(rebuildIfLastFailure: firstRun);

        // scan through files and see if they need to be run
        foreach (var relPath in _diagramFilesToRun)
        {
            ranFiles |= RunDiagramFileIfNeeded(relPath, rebuildIfLastFailure: firstRun);
        }

        if (ranFiles)
        {
            messageStopwatch.Start();
        }

        if (messageStopwatch.ElapsedMilliseconds > 1000)
        {
            if (_successTracker.SuccessfulSources.Count > 0)
            {
                _console.MarkupLine($"[green]Successful sources: {_successTracker.SuccessfulSources.Count}[/]");
            }

            if (_successTracker.FailedSources.Count > 0)
            {
                _console.MarkupLine("[red]Failed sources:[/]");
                foreach (var source in _successTracker.FailedSources)
                {
                    _console.MarkupLine($"- [red]{source}[/]");
                }
            }
            messageStopwatch.Reset();
            _console.QuietMarkupLine("Watching files for changes...\n");
        }
    }

    private bool RunCsxFilesIfNeeded(bool rebuildIfLastFailure = false)
    {
        bool someRan = false;

        foreach (var relPath in _csxFilesToRun)
        {
            someRan |= RunCsxFileIfNeeded(relPath, rebuildIfLastFailure);
        }

        return someRan;
    }

    private bool RunCsxFileIfNeeded(string csxRelPath, bool rebuildIfLastFailure = false)
    {
        bool scriptRan = false;

        try
        {
            _csxRunner.RunScriptIfNeeded(csxRelPath, _runInfoStore, out scriptRan, rebuildIfLastFailure);

            if (scriptRan)
            {
                _successTracker.AddSuccess(csxRelPath);
            }
        }
        catch (Exception)
        {
            _successTracker.AddFailure(csxRelPath);
            _console.ErrorMarkupLine($"Error running file: {csxRelPath}");
            //_console.WriteException(e);
        }
        
        if (scriptRan)
        {
            _console.WriteLine("");
            _runInfoDataBase.PersistRunInfo(_runInfoStore);
        }

        return scriptRan;
    }

    private bool RunDiagramFileIfNeeded(string diagramRelPath, bool rebuildIfLastFailure = false)
    {
        bool ranFile = false;

        var diagramAbsPath = MakeAbsolute(diagramRelPath);
        if (_runInfoStore.FindCsxWithDiagram(diagramAbsPath) != null)
        {
            return false;
        }
        _runInfoStore.diagramRuns.TryGetValue(diagramAbsPath, out var runInfo);

        if (runInfo == null)
        {
            runInfo = new DiagramRunInfo(diagramAbsPath);
            runInfo.lastCodeGenStartDateTime = DateTime.MinValue;
            runInfo.lastCodeGenEndDateTime = DateTime.MinValue;
        }

        var diagFileInfo = new FileInfo(diagramAbsPath);

        bool needsRun =
            (diagFileInfo.Exists && diagFileInfo.LastWriteTime >= runInfo.lastCodeGenStartDateTime) ||
            (rebuildIfLastFailure && runInfo.success == false);

        if (needsRun)
        {
            ranFile = true;

            try
            {
                _console.QuietMarkupLine($"Diagram `{diagramRelPath}` needs to be run");
                _diagramRunner.RunDiagramFile(shortPath: diagramRelPath, absolutePath: diagramAbsPath, out ranFile, _runInfoStore);
                if (ranFile)
                {
                    _successTracker.AddSuccess(diagramRelPath);
                }
            }
            catch (Exception)
            {
                _console.ErrorMarkupLine($"Error running diagram file: `{diagramRelPath}`");
                _successTracker.AddFailure(diagramRelPath);
                //_console.WriteException(e);
            }
            _console.WriteLine("");

            _runInfoDataBase.PersistRunInfo(_runInfoStore);
        }

        return ranFile;
    }

    private string MakeAbsolute(string path)
    {
        return Path.GetFullPath(Path.Combine(_searchDirectory, path));
    }

    private void SetupAndPrintCsxFiles()
    {
        if (_csxFilesToRun.Count > 0)
        {
            _console.MarkupLine("Watching the following csx files:");
            foreach (var csxFile in _csxFilesToRun)
            {
                _console.MarkupLine($"- {csxFile}");
            }
        }

        (string? versionString, Exception? exception) = DotnetScriptProgram.TryGetVersionString();
        if (versionString == null)
        {
            _console.ErrorMarkupLine($"Did not detect `{DotnetScriptProgram.Name}` program.");
            _console.WarnMarkupLine($"Not attempting to run StateSmith .csx scripts:");
            foreach (var path in _csxFilesToRun)
            {
                _console.WarnMarkupLine("    " + path);
            }
            _console.WriteLine("");

            if (IsVerbose)
            {
                _console.WriteLine("Attempted command to detect version:");
                _console.WriteException(exception!);
                _console.WriteLine("");
            }

            _csxFilesToRun.Clear();
        }
    }
}
