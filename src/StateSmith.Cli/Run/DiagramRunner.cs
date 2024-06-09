using Spectre.Console;
using StateSmith.Runner;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Cli.Run;

public class DiagramRunner
{
    //IAnsiConsole _console;
    RunConsole _runConsole;
    DiagramOptions _diagramOptions;
    RunInfo _runInfo;
    bool _forceRebuild = false;
    string _searchDirectory;

    public DiagramRunner(RunConsole runConsole, DiagramOptions diagramOptions, RunInfo runInfo, bool forceRebuild, string searchDirectory)
    {
        _runConsole = runConsole;
        this._diagramOptions = diagramOptions;
        _runInfo = runInfo;
        _forceRebuild = forceRebuild;
        _searchDirectory = searchDirectory;
    }

    public void Run(List<string> targetDiagramFiles)
    {
        if (_diagramOptions.Lang == TranspilerId.Default)
        {
            _runConsole.QuietMarkupLine("Not generating code for diagrams without csx files because no 'lang' specified.");
            return;
        }

        foreach (var diagramFile in targetDiagramFiles)
        {
            RunDiagramFile(diagramFile);
        }

        _runConsole.WriteLine("\nFinished running diagrams.");
    }

    private bool RunDiagramFile(string diagramShortPath)
    {
        bool diagramRan;

        string diagramLongerPath = $"{_searchDirectory}/{diagramShortPath}";
        string diagramAbsolutePath = Path.GetFullPath(diagramLongerPath);

        string? csxAbsPath = _runInfo.FindCsxWithDiagram(diagramAbsolutePath);
        if (csxAbsPath != null)
        {
            var csxRelativePath = Path.GetRelativePath(_searchDirectory, csxAbsPath);
            // TODO: only print if verbose
            _runConsole.QuietMarkupLine($"...Skipping diagram `{diagramShortPath}` already run by csx file `{csxRelativePath}`.");
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
            if (_forceRebuild)
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

        _runConsole.WriteLine($"Running diagram: `{diagramShortPath}`");
        diagramRan = true;

        SmRunner smRunner = new(diagramPath: diagramAbsolutePath, transpilerId: _diagramOptions.Lang);
        smRunner.Settings.simulation.enableGeneration = !_diagramOptions.NoSimGen; // enabled by default
        smRunner.Run();

        return diagramRan;
    }
}
