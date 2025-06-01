using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Cli.Run;

public class DiagramRunner
{
    private RunConsole _runConsole;
    private DiagramOptions _diagramOptions;

    private readonly string _searchDirectory;
    private readonly RunHandlerOptions _runHandlerOptions;
    private string CurrentDirectory => _runHandlerOptions.CurrentDirectory;

    public DiagramRunner(RunConsole runConsole, DiagramOptions diagramOptions, string searchDirectory, RunHandlerOptions runHandlerOptions)
    {
        _runConsole = runConsole;
        this._diagramOptions = diagramOptions;
        _searchDirectory = searchDirectory;
        this._runHandlerOptions = runHandlerOptions;
    }

    public void SetConsole(RunConsole runConsole)
    {
        _runConsole = runConsole;
    }

    public bool Run(List<string> targetDiagramFiles, RunInfoStore runInfoStore)
    {
        bool ranFiles = false;

        foreach (var diagramFile in targetDiagramFiles)
        {
            RunDiagramFileIfNeeded(diagramFile, runInfoStore, out var diagramRan);
            ranFiles |= diagramRan;
        }

        _runConsole.WriteLine("\nFinished running diagrams.");
        return ranFiles;
    }

    private bool IsVerbose => _runHandlerOptions.Verbose;
    private bool IsRebuild => _runHandlerOptions.Rebuild;

    public void RunDiagramFileIfNeeded(string diagramRelPath, RunInfoStore runInfoStore, out bool diagramRan, bool rebuildIfLastFailure = false)
    {
        string diagramLongerPath = $"{_searchDirectory}/{diagramRelPath}";
        string diagramAbsolutePath = Path.GetFullPath(diagramLongerPath);

        string? csxAbsPath = runInfoStore.FindCsxWithDiagram(diagramAbsolutePath);
        if (csxAbsPath != null)
        {
            var csxRelativePath = Path.GetRelativePath(_searchDirectory, csxAbsPath);
            _runConsole.QuietMarkupLine($"...Skipping diagram `{diagramRelPath}` already run by csx file `{csxRelativePath}`.", filter: IsVerbose);
            diagramRan = false;
            return;
        }

        _runConsole.AddMildHeader($"Checking diagram: `{diagramRelPath}`", filter: IsVerbose);
        _runConsole.WriteLine($"Diagram settings: {_diagramOptions.Describe()}", filter: IsVerbose);
        var incrementalRunChecker = new IncrementalRunChecker(_runConsole, _searchDirectory, IsVerbose, runInfoStore);

        if (incrementalRunChecker.TestDiagramOnlyFilePath(diagramAbsolutePath, rebuildIfLastFailure) != IncrementalRunChecker.Result.OkToSkip)
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
                _runConsole.QuietMarkupLine($"Diagram and its dependencies haven't changed. Skipping.", filter: IsVerbose);
                diagramRan = false;
                return; //!!!!!!!!!!! NOTE the return here.
            }
        }

        RunDiagramFile(diagramRelPath, diagramAbsolutePath, out diagramRan, runInfoStore);
    }

    public void RunDiagramFile(string shortPath, string absolutePath, out bool diagramRan, RunInfoStore runInfoStore)
    {
        string callerFilePath = CurrentDirectory + "/";  // Slash needed for fix of https://github.com/StateSmith/StateSmith/issues/345

        RunnerSettings runnerSettings = new(diagramFile: absolutePath, transpilerId: _diagramOptions.Lang);
        runnerSettings.simulation.enableGeneration = !_diagramOptions.NoSimGen; // enabled by default
        runnerSettings.propagateExceptions = _runHandlerOptions.PropagateExceptions;
        runnerSettings.dumpErrorsToFile = _runHandlerOptions.DumpErrorsToFile;

        var info = new DiagramRunInfo(absolutePath: absolutePath);
        runInfoStore.diagramRuns[absolutePath] = info; // will overwrite if already exists

        // the constructor will attempt to read diagram settings from the diagram file
        SmRunner smRunner = new(settings: runnerSettings, renderConfig: null, callerFilePath: callerFilePath);
        smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter, LoggingCodeFileWriter>();

        if (smRunner.PreDiagramBasedSettingsException != null)
        {
            _runConsole.ErrorMarkupLine("\nFailed while trying to read diagram for settings.\n");
            smRunner.PrintAndThrowIfPreDiagramSettingsException();   // need to do this before we check the transpiler ID
            throw new Exception("Should not get here.");
        }

        _runConsole.WriteLine($"Running diagram: `{shortPath}`");

        if (runnerSettings.transpilerId == TranspilerId.NotYetSet)
        {
            _runConsole.MarkupLine($"Ignoring diagram as no language specified `--lang` and no transpiler ID found in diagram.");
            diagramRan = false;
            return; //!!!!!!!!!!! NOTE the return here.
        }

        LoggingCodeFileWriter loggingCodeFileWriter = (LoggingCodeFileWriter)smRunner.GetExperimentalAccess().DiServiceProvider.GetInstanceOf<ICodeFileWriter>();

        try
        {
            diagramRan = true;
            smRunner.Run();
            info.success = true;
            info.lastCodeGenEndDateTime = DateTime.Now;
        }
        finally
        {
            info.writtenFileAbsolutePaths.AddRange(loggingCodeFileWriter.filePathsWritten);
        }
    }
}
