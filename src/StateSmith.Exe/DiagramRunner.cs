using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;

namespace StateSmith.Exe;

public class DiagramRunner
{
    private IAnsiConsole _console;
    private ProgramOptions _options;

    public DiagramRunner(IAnsiConsole console, ProgramOptions options)
    {
        _console = console;
        _options = options;
    }

    public void Run(List<string> files)
    {
        foreach (var file in files)
        {
            ProcessFile(file);
        }
    }

    public void ProcessFile(string filePath)
    {
        RunnerSettings runnerSettings = new(diagramFile: Path.GetFullPath(filePath), transpilerId: _options.Lang);
        runnerSettings.simulation.enableGeneration = !_options.NoSimGen; // enabled by default

        // the constructor will attempt to read diagram settings from the diagram file
        SmRunner smRunner = new(settings: runnerSettings, renderConfig: null);
        smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter, LoggingCodeFileWriter>();

        if (smRunner.PreDiagramBasedSettingsException != null)
        {
            _console.WriteLine($"Failed while trying to read '`{filePath}`' for settings.");
            smRunner.PrintAndThrowIfPreDiagramSettingsException();   // need to do this before we check the transpiler ID
            throw new Exception("Should not get here.");
        }

        _console.WriteLine($"Running diagram: `{filePath}`");

        if (runnerSettings.transpilerId == TranspilerId.NotYetSet)
        {
            _console.MarkupLine($"Ignoring diagram as no language specified `--lang` and no transpiler ID found in diagram.");
            return; 
        }

        LoggingCodeFileWriter loggingCodeFileWriter = (LoggingCodeFileWriter)smRunner.GetExperimentalAccess().DiServiceProvider.GetInstanceOf<ICodeFileWriter>();

        smRunner.Run();
    }
}
