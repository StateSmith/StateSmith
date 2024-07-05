using Spectre.Console;
using StateSmith.Cli.Manifest;
using StateSmith.Cli.Setup;
using StateSmith.Cli.Utils;
using StateSmith.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Cli.Run;

public class RunHandler
{
    public SsCsxDiagramFileFinder Finder;

    private CsxOutputParser _parser;
    private IManifestPersistance _manifestPersistance;
    private string manifestDirectory;

    RunInfo _runInfo;
    internal IncrementalRunChecker _incrementalRunChecker;
    internal RunInfoDataBase _runInfoDataBase;
    bool _forceRebuild = false;
    IAnsiConsole _console;
    private readonly DiagramOptions _diagramOptions;
    RunConsole _runConsole;
    bool _verbose;
    bool _noCsx;

    public RunHandler(IAnsiConsole console, string dirOrManifestPath, DiagramOptions diagramOptions, bool verbose, bool noCsx, IManifestPersistance? manifestPersistance = null)
    {
        _console = console;
        this._diagramOptions = diagramOptions;
        dirOrManifestPath = Path.GetFullPath(dirOrManifestPath);

        FileAttributes attr = File.GetAttributes(dirOrManifestPath);
        if (attr.HasFlag(FileAttributes.Directory))
            dirOrManifestPath = PathUtils.EnsureDirEndingSeperator(dirOrManifestPath);

        manifestDirectory = Path.GetDirectoryName(dirOrManifestPath).ThrowIfNull();

        _parser = new CsxOutputParser();
        _runInfo = new RunInfo(dirOrManifestPath);
        _runInfoDataBase = new RunInfoDataBase(dirOrManifestPath, console);
        _incrementalRunChecker = new IncrementalRunChecker(_console, manifestDirectory, verbose);
        Finder = new SsCsxDiagramFileFinder();
        _manifestPersistance = manifestPersistance ?? new ManifestPersistance(manifestDirectory);
        this._verbose = verbose;
        _runConsole = new RunConsole(_console);
        _noCsx = noCsx;
    }

    public void SetForceRebuild(bool forceRebuild)
    {
        _forceRebuild = forceRebuild;
    }

    public void CreateBlankManifest()
    {
        var manifest = new ManifestData();

        foreach (var ext in StandardFiles.GetStandardFileExtensions())
        {
            manifest.RunManifest.IncludePathGlobs.Add($"**/*{ext}");
        }

        WriteManifest(manifest);
    }

    private void WriteManifest(ManifestData manifest)
    {
        if (_manifestPersistance.ManifestExists() && UiHelper.AskForOverwrite(_console) == false)
        {
            return;
        }

        _manifestPersistance.Write(manifest, overWrite: true);
        _console.MarkupLine($"Manifest written successfully to [green]{ManifestPersistance.ManifestFileName}[/].");
    }

    public void Run()
    {
        try
        {
            RunInner();
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            //throw;
        }
    }

    private void RunInner()
    {
        string searchDirectory = manifestDirectory;

        ReadPastRunInfoDatabase();
        var scanResults = Finder.Scan(searchDirectory: searchDirectory);
        RunScriptsIfNeeded(scanResults.targetCsxFiles);

        var diagramRunner = new DiagramRunner(_runConsole, _diagramOptions, _runInfo, _forceRebuild, searchDirectory: searchDirectory, verbose: _verbose);
        diagramRunner.Run(scanResults.targetDiagramFiles);

        PrintScanInfo(scanResults);
    }

    private void PrintScanInfo(SsCsxDiagramFileFinder.ScanResults scanResults)
    {
        bool spacerPrinted = false;

        void PrintSpacerIfNeeded()
        {
            if (!spacerPrinted)
            {
                _runConsole.MarkupLine("");
                spacerPrinted = true;
            }
        }

        // print ignored files
        if (_verbose && scanResults.ignoredFiles.Count > 0)
        {
            PrintSpacerIfNeeded();
            _runConsole.QuietMarkupLine("Ignored files: " + string.Join(", ", scanResults.ignoredFiles));
        }

        // print non-matching files
        if (_verbose && scanResults.nonMatchingFiles.Count > 0)
        {
            PrintSpacerIfNeeded();
            _runConsole.QuietMarkupLine("Non-matching files: " + string.Join(", ", scanResults.nonMatchingFiles));
        }

        // print broken svg files always (ignore verbose)
        if (scanResults.brokenDrawioSvgFiles.Count > 0)
        {
            _runConsole.WriteLine(""); // always add a spacer
            _runConsole.WarnMarkupLine("!!! Broken drawio.svg files found !!!");

            foreach (var item in scanResults.brokenDrawioSvgFiles)
            {
                _runConsole.MarkupLine($"  - {item}");
            }

            _runConsole.MarkupLine("  - see [blue][u]https://github.com/StateSmith/StateSmith/issues/341[/][/]");
        }
    }

    public void RunScriptsIfNeeded(List<string> csxScripts)
    {
        if (_noCsx)
        {
            if (_verbose)
            {
                _runConsole.QuietMarkupLine("Ignoring all .csx scripts for --no-csx.");
            }
            return;
        }

        if (csxScripts.Count == 0)
        {
            _console.MarkupLine("No .csx scripts found to run.");
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

            if (_verbose)
            {
                _runConsole.WriteLine("Attempted command to detect version:");
                _runConsole.WriteException(exception!);
                _runConsole.WriteLine("");

                _runConsole.MarkupLine("You can ignore .csx files with the [green]--no-csx[/] CLI option.");
            }

            return;
        }

        if (_verbose)
        {
            _runConsole.QuietMarkupLine($"Running StateSmith .csx scripts with `{DotnetScriptProgram.Name}` version: " + versionString);
        }

        bool anyScriptsRan = false;

        foreach (var csxShortPath in csxScripts)
        {
            anyScriptsRan |= RunScriptIfNeeded(searchDirectory: manifestDirectory, csxShortPath);
            //_console.WriteLine(); // already lots of newlines in RunScriptIfNeeded
        }

        if (!anyScriptsRan)
        {
            _console.WriteLine("No scripts needed to be run.");
        }
        else
        {
            _console.WriteLine("Finished running scripts.");
            _runInfoDataBase.PersistRunInfo(_runInfo);
        }
    }

    private bool RunScriptIfNeeded(string searchDirectory, string csxShortPath)
    {
        bool scriptRan = false;
        string csxLongerPath = $"{searchDirectory}/{csxShortPath}";
        string csxAbsolutePath = Path.GetFullPath(csxLongerPath);

        _runConsole.AddMildHeader($"Checking script and diagram dependencies for: `{csxShortPath}`");
        IncrementalRunChecker.Result runCheck = _incrementalRunChecker.TestFilePath(csxAbsolutePath);
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
                _runConsole.QuietMarkupLine($"Script and its diagram dependencies haven't changed. Skipping.");
                return scriptRan; //!!!!!!!!!!! NOTE the return here.
            }
        }

        _console.WriteLine($"Running script: `{csxShortPath}`");
        scriptRan = true;

        SimpleProcess process = new()
        {
            WorkingDirectory = searchDirectory,
            SpecificCommand = DotnetScriptProgram.Name,
            SpecificArgs = csxAbsolutePath,
            throwOnExitCode = true
        };
        process.EnableEchoToTerminal(_console);

        // Important that we grab time before running the process.
        // This ensures that we can detect if diagram or csx file was modified after our run.
        var info = new CsxRunInfo(csxAbsolutePath: csxAbsolutePath);
        process.Run(timeoutMs: 60000);
        info.lastCodeGenEndDateTime = DateTime.Now;

        _parser.ParseAndResolveFilePaths(process.StdOutput, info);
        _runInfo.csxRuns[csxAbsolutePath] = info; // will overwrite if already exists
        return scriptRan;
    }

    private void ReadPastRunInfoDatabase()
    {
        RunInfo? readRunInfo = _runInfoDataBase.ReadRunInfoDatabase();
        _incrementalRunChecker.SetReadRunInfo(readRunInfo);

        if (readRunInfo != null)
        {
            _runInfo = readRunInfo.DeepCopy();
        }
    }

    public void AddFromManifest(ManifestData manifest)
    {
        Finder.AddIncludePatterns(manifest.RunManifest.IncludePathGlobs);
        Finder.AddExcludePatterns(manifest.RunManifest.ExcludePathGlobs);
    }

    [Obsolete("This method will probably be removed soon")]
    public void ScanAndCreateManifest()
    {
        var throwNotImplemented = true;
        // not quite ready to remove this code yet

        if (throwNotImplemented)
            throw new NotImplementedException();

        var manifest = new ManifestData();

        //var csxScripts = Finder.Scan(searchDirectory: manifestDirectory);
        //foreach (var csxRelativePath in csxScripts)
        //{
        //    //manifest.RunManifest.ManuallySpecifiedProjects.Add(new ProjectSetting(csxRelativePath));
        //}

        WriteManifest(manifest);
    }
}
