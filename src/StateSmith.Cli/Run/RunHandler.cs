using Spectre.Console;
using StateSmith.Common;
using StateSmithTest.Processes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StateSmith.Cli.Run;

public class RunHandler
{
    private CsxOutputParser _parser;
    RunInfo _runInfo;
    private SsCsxFileFinder _finder;
    internal IncrementalRunChecker _incrementalRunChecker;
    internal RunInfoDataBase _runInfoDataBase;
    bool _forceRebuild = false;
    private Manifest? manifest;
    string dirOrManifestPath;
    private ManifestPersistance _manifestPersistance;
    IAnsiConsole _console = AnsiConsole.Console;
    private string manifestDirectory;

    public RunHandler(Manifest? manifest, string dirOrManifestPath)
    {
        dirOrManifestPath = Path.GetFullPath(dirOrManifestPath);

        FileAttributes attr = File.GetAttributes(dirOrManifestPath);
        if (attr.HasFlag(FileAttributes.Directory))
            dirOrManifestPath = PathUtils.EnsureDirEndingSeperator(dirOrManifestPath);

        manifestDirectory = Path.GetDirectoryName(dirOrManifestPath).ThrowIfNull();

        _parser = new CsxOutputParser();
        _runInfo = new RunInfo(dirOrManifestPath);
        _runInfoDataBase = new RunInfoDataBase(dirOrManifestPath);
        _incrementalRunChecker = new IncrementalRunChecker(_console, manifestDirectory);
        _finder = new SsCsxFileFinder();
        this.manifest = manifest;
        this.dirOrManifestPath = dirOrManifestPath;
        _manifestPersistance = new ManifestPersistance(manifestDirectory);
    }

    public void SetConsole(IAnsiConsole console)
    {
        _console = console;
    }

    public void SetForceRebuild(bool forceRebuild)
    {
        _forceRebuild = forceRebuild;
    }

    /// <summary>
    /// Ignores null arguments
    /// </summary>
    /// <param name="path"></param>
    public void IgnorePath(string? path)
    {
        if (path == null)
            return;

        _finder.AddExcludePattern(path);
    }

    public void CreateBlankManifest()
    {
        manifest = new Manifest();
        _manifestPersistance.Write(manifest, overWrite: true);
    }

    public void ScanAndCreateManifest()
    {
        manifest = new Manifest();

        var csxScripts = _finder.Scan(searchDirectory: manifestDirectory);
        foreach (var csxRelativePath in csxScripts)
        {
            manifest.RunManifest.AutoDiscoveredProjects.Add(new ProjectSetting(csxRelativePath));
        }

        _manifestPersistance.Write(manifest, overWrite: true);
    }

    public void Run(bool recursive = false)
    {
        _console.MarkupLine($"[cyan]This feature Still a work in progress...[/]");

        ReadPastRunInfoDatabase();

        if (manifest == null)
        {
            new RunUi(this).HandleNoManifest();
        }
        else
        {
            
        }


        if (recursive)
            _finder.SetAsRecursive();

        var csxScripts = _finder.Scan(searchDirectory: manifestDirectory);

        RunScriptsIfNeeded(csxScripts);
    }

    public void RunScriptsIfNeeded(List<string> csxScripts)
    {
        bool anyScriptsRan = false;

        foreach (var csxShortPath in csxScripts)
        {
            anyScriptsRan |= RunScriptIfNeeded(manifestDirectory, csxShortPath);
            Console.WriteLine();
        }

        if (!anyScriptsRan)
        {
            Console.WriteLine("No scripts needed to be run.");
        }
        else
        {
            Console.WriteLine("Finished running scripts.");
            _runInfoDataBase.PersistRunInfo(_runInfo);
        }
    }

    private bool RunScriptIfNeeded(string searchDirectory, string csxShortPath)
    {
        bool scriptRan = false;
        string csxLongerPath = $"{searchDirectory}/{csxShortPath}";
        string csxAbsolutePath = Path.GetFullPath(csxLongerPath);

        AddMildHeader($"Checking script and diagram dependencies for: `{csxShortPath}`");
        IncrementalRunChecker.Result runCheck = _incrementalRunChecker.TestFilePath(csxAbsolutePath);
        if (runCheck != IncrementalRunChecker.Result.OkToSkip)
        {
            // already basically printed by IncrementalRunChecker
            //Console.WriteLine($"Script or its diagram dependencies have changed. Running script.");
        }
        else
        {
            if (_forceRebuild)
            {
                ConsoleMarkupLine("Would normally skip (file dates look good), but [yellow]rebuild[/] option set.");
            }
            else
            {
                QuietMarkupLine($"Script and its diagram dependencies haven't changed. Skipping.");
                return scriptRan; //!!!!!!!!!!! NOTE the return here.
            }
        }

        Console.WriteLine($"Running script: `{csxShortPath}`");
        scriptRan = true;

        SimpleProcess process = new()
        {
            WorkingDirectory = searchDirectory,
            SpecificCommand = "dotnet-script",
            SpecificArgs = csxAbsolutePath,
            throwOnExitCode = true
        };
        process.EnableEchoToTerminal();

        // Important that we grab time before running the process.
        // This ensures that we can detect if diagram or csx file was modified after our run.
        var info = new CsxRunInfo(csxAbsolutePath: csxAbsolutePath);
        process.Run(timeoutMs: 6000);

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

    private void AddMildHeader(string header)
    {
        _console.MarkupLine("");

        var rule = new Rule($"[blue]{header}[/]")
        {
            Justification = Justify.Left
        };
        _console.Write(rule);
    }

    private void QuietMarkupLine(string message)
    {
        ConsoleMarkupLine($"[grey]{message}[/]");
    }

    private void ConsoleMarkupLine(string message)
    {
        _console.MarkupLine(message);
    }
}

/*

ss.cli run --here



ss.cli run

    > no statesmith manifest found. What do you want to do?
    >> automatically create a manifest file here
    >> search up the directory tree for manifest and run that (limit of X directories)
    >> find and run StateSmith csx files in this directory
    >> exit


statesmith.manifest.json
{
    DiscoveredProjects: [
        {
            CsxPath: "./path/to/project/RocketSm.csx",
            DiagramPath: "./path/to/project/RocketSm.csx",
            AlwaysBuild: false
        }
    ],
}



ss.cli run

> reading statesmith.manifest.json
> found 3 project to build
> project 1 of 3: C:\path\to\project.csx
> project is already up to date. skipping.
> project 2 of 3: C:\path\to\project2.csx
> project needs to be built. building.



ss.cli run --discover --recursive
> respects the `PathsToIgnoreForDiscovery` list
> updates the manifest file with the new projects found

ss.cli run --force-build
> forces a rebuild of all projects


*/
