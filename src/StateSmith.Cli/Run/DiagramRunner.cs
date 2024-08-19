using Spectre.Console;
using StateSmith.Output.Gil;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Cli.Run;

public class DiagramRunner
{
    //IAnsiConsole _console;
    RunConsole _runConsole;
    DiagramOptions _diagramOptions;
    RunInfo _runInfo;
    readonly string _searchDirectory;
    private readonly RunHandlerOptions _runHandlerOptions;
    private string CurrentDirectory => _runHandlerOptions.CurrentDirectory;

    public DiagramRunner(RunConsole runConsole, DiagramOptions diagramOptions, RunInfo runInfo, string searchDirectory, RunHandlerOptions runHandlerOptions)
    {
        _runConsole = runConsole;
        this._diagramOptions = diagramOptions;
        _runInfo = runInfo;
        _searchDirectory = searchDirectory;
        this._runHandlerOptions = runHandlerOptions;
    }

    public void Run(List<string> targetDiagramFiles)
    {
        foreach (var diagramFile in targetDiagramFiles)
        {
            RunDiagramFile(diagramFile);
        }

        _runConsole.WriteLine("\nFinished running diagrams.");
    }

    private bool IsVerbose => _runHandlerOptions.Verbose;
    private bool IsRebuild => _runHandlerOptions.Rebuild;

    private bool RunDiagramFile(string diagramShortPath)
    {
        bool diagramRan;

        string diagramLongerPath = $"{_searchDirectory}/{diagramShortPath}";
        string diagramAbsolutePath = Path.GetFullPath(diagramLongerPath);

        string? csxAbsPath = _runInfo.FindCsxWithDiagram(diagramAbsolutePath);
        if (csxAbsPath != null)
        {
            var csxRelativePath = Path.GetRelativePath(_searchDirectory, csxAbsPath);
            if (IsVerbose)
            {
                _runConsole.QuietMarkupLine($"...Skipping diagram `{diagramShortPath}` already run by csx file `{csxRelativePath}`.");
            }
            diagramRan = false;
            return diagramRan;
        }

        _runConsole.AddMildHeader($"Checking diagram: `{diagramShortPath}`");
        _runConsole.WriteLine($"Diagram settings: {_diagramOptions.Describe()}");
        _runConsole.QuietMarkupLine($"Change detection not implemented yet. Rebuild for diagram. Issue #328.");
        // TODO: https://github.com/StateSmith/StateSmith/issues/328
        // Need to actually check something like `_incrementalRunChecker.TestFilePath(absolutePath);`
        IncrementalRunChecker.Result runCheck = IncrementalRunChecker.Result.NeedsRunNoInfo;
        if (runCheck != IncrementalRunChecker.Result.OkToSkip)
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
                _runConsole.QuietMarkupLine($"Diagram and its dependencies haven't changed. Skipping.");
                diagramRan = false;
                return diagramRan; //!!!!!!!!!!! NOTE the return here.
            }
        }

        string callerFilePath = CurrentDirectory + "/";  // Slash needed for fix of https://github.com/StateSmith/StateSmith/issues/345

        RunnerSettings runnerSettings = new(diagramFile: diagramAbsolutePath, transpilerId: _diagramOptions.Lang);
        runnerSettings.simulation.enableGeneration = !_diagramOptions.NoSimGen; // enabled by default
        runnerSettings.propagateExceptions = _runHandlerOptions.PropagateExceptions;
        runnerSettings.dumpErrorsToFile = _runHandlerOptions.DumpErrorsToFile;

        // the constructor will attempt to read diagram settings from the diagram file
        SmRunner smRunner = new(settings: runnerSettings, renderConfig: null, callerFilePath: callerFilePath);

        if (smRunner.PreDiagramBasedSettingsException != null)
        {
            _runConsole.ErrorMarkupLine("\nFailed while trying to read diagram for settings.\n");
            smRunner.PrintAndThrowIfPreDiagramSettingsException();   // need to do this before we check the transpiler ID
            throw new Exception("Should not get here.");
        }

        if (runnerSettings.transpilerId == TranspilerId.NotYetSet)
        {
            _runConsole.MarkupLine($"Ignoring diagram as no language specified `--lang` and no transpiler ID found in diagram.");
            diagramRan = false;
            return diagramRan; //!!!!!!!!!!! NOTE the return here.
        }

        _runConsole.WriteLine($"Running diagram: `{diagramShortPath}`");
        smRunner.Run();
        diagramRan = true;

        return diagramRan;
    }
}
