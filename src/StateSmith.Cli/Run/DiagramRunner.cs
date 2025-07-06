using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmith.Cli.Run;

public class DiagramRunner
{
    // PUBLIC VAR! Feel free to clear it.
    public int warningCount = 0;

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

        if (targetDiagramFiles.Count == 0)
        {
            _runConsole.MarkupLine("No diagrams found (that aren't already run by .csx).", filter: IsVerbose);
        }

        foreach (var diagramFile in targetDiagramFiles)
        {
            RunDiagramFileIfNeeded(diagramFile, runInfoStore, out var diagramRan);
            ranFiles |= diagramRan;
        }

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
        var sp = RunnerServiceProviderFactory.CreateDefault((services) =>
        {
            services.AddSingleton<ICodeFileWriter, LoggingCodeFileWriter>();
        });
        
        SmRunner smRunner = new(settings: runnerSettings, renderConfig: null, callerFilePath: callerFilePath, serviceProvider: sp);


        _runConsole.WriteLine($"Running diagram: `{shortPath}`");

        if (runnerSettings.transpilerId == TranspilerId.NotYetSet)
        {
            _runConsole.WarnMarkupLine($"Ignoring diagram as no language specified `--lang` and no transpiler ID found in diagram.");
            warningCount++;
            diagramRan = false;
            return; //!!!!!!!!!!! NOTE the return here.
        }

        // note that this cast above is OK because we registered the LoggingCodeFileWriter above
        LoggingCodeFileWriter loggingCodeFileWriter = (LoggingCodeFileWriter)smRunner.GetExperimentalAccess().IServiceProvider.GetRequiredService<ICodeFileWriter>();

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
