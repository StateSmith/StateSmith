using Spectre.Console;
using StateSmith.Cli.Manifest;
using StateSmith.Cli.Utils;
using System;
using System.IO;

namespace StateSmith.Cli.Run;

public class RunUi
{
    IAnsiConsole _console;
    RunOptions opts;
    RunHandler runHandler;
    readonly string currentDirectory;

    public RunUi(RunOptions opts, IAnsiConsole _console, string currentDirectory)
    {
        this.opts = opts;
        this.currentDirectory = currentDirectory;
        runHandler = new(_console, currentDirectory, opts.GetDiagramOptions(), opts.GetRunHandlerOptions(currentDirectory: currentDirectory));
        this._console = _console;
    }

    public int HandleRunCommand()
    {
        _console.MarkupLine("");
        UiHelper.AddSectionLeftHeader(_console, "Run");

        SetRunHandlerFromOptions();

        if (opts.Menu)
        {
            AskWhatToDo("What did you want to do?");
            return 0;
        }

        if (opts.Here)
        {
            // ignore any manifest file
            runHandler.Finder.AddDefaultIncludePatternIfNone();
            runHandler.Run();
            return 0;
        }

        if (opts.Up)
        {
            SearchUpForManifestAndRun();
            return 0;
        }

        var manPersistance = new ManifestPersistance(currentDirectory);
        ManifestData? manifest = null;

        if (manPersistance.ManifestExists())
        {
            try
            {
                manifest = manPersistance.ReadOrThrow();
                _console.MarkupLine($"[green]Using manifest file:[/] {ManifestPersistance.ManifestFileName}");
            }
            catch (Exception e)
            {
                _console.MarkupLine($"[red]Error reading manifest file:[/] {ManifestPersistance.ManifestFileName}");
                _console.WriteException(e);

                // let it fall through and it will show the menu
            }
        }

        if (manifest == null)
        {
            AskWhatToDo();
            return 0;
        }
        else
        {
            runHandler.AddFromManifest(manifest);
            runHandler.Run();
            return 0;
        }
    }

    private void SetRunHandlerFromOptions()
    {
        // add patterns from command line
        runHandler.Finder.AddIncludePatterns(opts.IncludePatterns);
        runHandler.Finder.AddExcludePatterns(opts.ExcludePatterns);

        if (opts.Recursive)
            runHandler.Finder.SetAsRecursive();
    }

    private void AskWhatToDo(string? title = null)
    {
        const string findAndRunCsxFiles = "[cyan]Run Here.[/] Find and run StateSmith csx files here. No manifest used or created.";
        const string findAndRunCsxFilesRecursively = "[cyan]Run Here Recursively.[/] Find and run StateSmith csx files here recursively. No manifest used or created.";
        const string searchUpForManifest = "[cyan]Search Upwards For Manifest[/] and use that.";
        //const string autoCreateManifestFile = "[cyan]Create Manifest From Scan.[/] Recursively finds StateSmith csx files in this directory and stores in manifest.";
        const string blankManifest = "[cyan]Create Default Manifest[/] file here. Useful for filtering files in large directories.";
        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title(title ?? $"No valid manifest ({ManifestPersistance.ManifestFileName}) found. What did you want to do?")
            .AddChoices(new[] {
                findAndRunCsxFilesRecursively,
                findAndRunCsxFiles,
                searchUpForManifest,
                //autoCreateManifestFile,
                blankManifest,
            }));

        switch (choice)
        {
            case findAndRunCsxFiles:
                runHandler.Finder.AddDefaultIncludePatternIfNone();
                runHandler.Run();
                break;
            case findAndRunCsxFilesRecursively:
                runHandler.Finder.AddDefaultIncludePatternIfNone();
                runHandler.Finder.SetAsRecursive();
                runHandler.Run();
                break;
            case searchUpForManifest:
                SearchUpForManifestAndRun();
                break;
            //case autoCreateManifestFile:
            //    runHandler.Finder.SetAsRecursive();
            //    runHandler.ScanAndCreateManifest();
            //    break;
            case blankManifest:
                runHandler.CreateBlankManifest();
                break;
        }
    }

    internal void SearchUpForManifestAndRun()
    {
        DirectoryInfo? dir = new(currentDirectory);

        while (dir != null && !ManifestPersistance.ManifestExists(dir.FullName))
        {
            dir = dir.Parent;
        }

        if (dir == null)
        {
            throw new FileNotFoundException($"Could not find a manifest file ({ManifestPersistance.ManifestFileName}) in this or parent directories.");
        }

        _console.MarkupLine($"Found manifest in directory: {dir.FullName}");

        ManifestData manifest = new ManifestPersistance(dir.FullName).ReadOrThrow();
        runHandler = new RunHandler(_console, dir.FullName, opts.GetDiagramOptions(), opts.GetRunHandlerOptions(currentDirectory: currentDirectory));
        SetRunHandlerFromOptions();
        runHandler.AddFromManifest(manifest);
        runHandler.Run();
    }
}
