using Spectre.Console;
using StateSmith.Cli.Utils;
using StateSmith.Common;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StateSmith.Cli.Run;

public class RunLiteHandler
{

    private CsxOutputParser _parser;

    IAnsiConsole _console;
    private readonly DiagramOptions _diagramOptions;
    RunConsole _runConsole;
    RunLiteOptions _runLiteOptions;
    private bool IsVerbose => _runLiteOptions.Verbose;

    public RunLiteHandler(IAnsiConsole console, DiagramOptions diagramOptions, RunLiteOptions runLiteOptions)
    {
        _console = console;
        _runLiteOptions = runLiteOptions;
        this._diagramOptions = diagramOptions;

        _parser = new CsxOutputParser();
        _runConsole = new RunConsole(_console);
    }



    public void Run()
    {
        try
        {
            RunInner();
        }
        catch (Exception ex)
        {
            if (ex is not FinishedWithFailureException)
            {
                _console.WriteException(ex);
            }

            Environment.ExitCode = 1;   // TODO - fix. this is not ideal. Might mess up unit tests.
        }
    }

    private void RunInner()
    {
        RunFiles(_runLiteOptions.Files);

        if( _runLiteOptions.Watch) {
            var watchers = new List<FileSystemWatcher>();
            foreach (var diagramFile in _runLiteOptions.Files)
            {
                var watcher = new FileSystemWatcher();
                watchers.Add(watcher); // don't let watchers go out of scope yet
                var path = Path.GetDirectoryName(diagramFile);
                watcher.Path = path!=null && path.Length>0 ? path : "."; 
                watcher.Filter = Path.GetFileName(diagramFile);
                watcher.Changed += (sender, e) => 
                {
                    _runConsole.WriteLine($"File {diagramFile} has changed.");
                    RunFile(diagramFile);
                };
                watcher.EnableRaisingEvents = true;            
            }

            Console.WriteLine("Watching for changes. Press enter to exit.");
            Console.ReadLine();
        }
    }

    private void RunFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            RunFile(file);
        }
    }

    private void RunFile(String file) 
    {
        foreach( var extension in DiagramFileAssociator.GetAllDiagramExtensions() ) {
            if( file.ToLower().EndsWith(extension.ToLower())) {
                RunDiagramFile(file);
                return;
            }
        }

        if(SsCsxFilter.IsScriptFile(file)) {
            RunCsxFile(file);
        } else {
            _runConsole.ErrorMarkupLine($"Unknown file type: {file}");
        }
    }

    private void RunCsxFile(String file) {
        AssertDotNetAvailable();

        string csxAbsolutePath = Path.GetFullPath(file);

        _console.WriteLine($"Running script: `{file}`");

        SimpleProcess process = new()
        {
            WorkingDirectory = ".",
            SpecificCommand = DotnetScriptProgram.Name,
            SpecificArgs = csxAbsolutePath,
            throwOnExitCode = false
        };
        process.EnableEchoToTerminal(_console);

        process.Run(timeoutMs: 60000);
        if (process.GetExitCode() != 0)
        {
            throw new FinishedWithFailureException();
        }
    }

    private void RunDiagramFile(string file) {
        var tmp = new DiagramRunner(
            _runConsole,
            _diagramOptions,
            null,
            searchDirectory: ".",
            new RunHandlerOptions(".")
            {
                Verbose = IsVerbose,
                NoCsx = false,
                Rebuild = false
            }
            );
        tmp.Run((new string[] { file }).ToList() );
    }

    private void AssertDotNetAvailable() 
    {
        (string? versionString, Exception? exception) = DotnetScriptProgram.TryGetVersionString();
        if (versionString == null)
        {
            _runConsole.ErrorMarkupLine($"Did not detect `{DotnetScriptProgram.Name}` program.");
            _runConsole.WarnMarkupLine($"Not attempting to run StateSmith .csx scripts:");

            // TODO handle better
            throw new FinishedWithFailureException();
        }   
    }
}
