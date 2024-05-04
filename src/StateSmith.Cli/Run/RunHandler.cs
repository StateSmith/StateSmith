using Spectre.Console;
using StateSmith.Cli.Utils;
using StateSmith.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StateSmith.Cli.Run;

public class RunHandler
{
    private CsxOutputParser _parser;
    RunInfo _runInfo;
    public SsCsxFileFinder Finder;
    internal IncrementalRunChecker _incrementalRunChecker;
    internal RunInfoDataBase _runInfoDataBase;
    bool _forceRebuild = false;
    string dirOrManifestPath;
    private ManifestPersistance _manifestPersistance;
    IAnsiConsole _console;
    private string manifestDirectory;

    public RunHandler(IAnsiConsole console, string dirOrManifestPath)
    {
        _console = console;
        dirOrManifestPath = Path.GetFullPath(dirOrManifestPath);

        FileAttributes attr = File.GetAttributes(dirOrManifestPath);
        if (attr.HasFlag(FileAttributes.Directory))
            dirOrManifestPath = PathUtils.EnsureDirEndingSeperator(dirOrManifestPath);

        manifestDirectory = Path.GetDirectoryName(dirOrManifestPath).ThrowIfNull();

        _parser = new CsxOutputParser();
        _runInfo = new RunInfo(dirOrManifestPath);
        _runInfoDataBase = new RunInfoDataBase(dirOrManifestPath);
        _incrementalRunChecker = new IncrementalRunChecker(_console, manifestDirectory);
        Finder = new SsCsxFileFinder();
        this.dirOrManifestPath = dirOrManifestPath;
        _manifestPersistance = new ManifestPersistance(manifestDirectory);
    }

    public void SetForceRebuild(bool forceRebuild)
    {
        _forceRebuild = forceRebuild;
    }

    public void CreateBlankManifest()
    {
        var manifest = new Manifest();
        manifest.RunManifest.IncludePathGlobs.Add("**/*.csx");
        WriteManifest(manifest);
    }

    private void WriteManifest(Manifest manifest)
    {
        if (_manifestPersistance.ManifestExists() && UiHelper.AskForOverwrite(_console) == false)
        {
            return;
        }

        _manifestPersistance.Write(manifest, overWrite: true);
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
        //_console.MarkupLine($"[cyan]This feature Still a work in progress...[/]");

        ReadPastRunInfoDatabase();
        var csxScripts = Finder.Scan(searchDirectory: manifestDirectory);
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

    public void AddFromManifest(Manifest manifest)
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

        var manifest = new Manifest();

        var csxScripts = Finder.Scan(searchDirectory: manifestDirectory);
        foreach (var csxRelativePath in csxScripts)
        {
            //manifest.RunManifest.ManuallySpecifiedProjects.Add(new ProjectSetting(csxRelativePath));
        }

        WriteManifest(manifest);
    }
}
