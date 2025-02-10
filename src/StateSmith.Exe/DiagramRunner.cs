using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Exe;

public class DiagramRunner
{
    private RunConsole _runConsole;
    private DiagramOptions _diagramOptions;

    public DiagramRunner(RunConsole runConsole, DiagramOptions diagramOptions)
    {
        _runConsole = runConsole;
        _diagramOptions = diagramOptions;
    }

    public bool Run(List<string> diagramFiles)
    {
        foreach (var diagramFile in diagramFiles)
        {
            RunDiagramFile(diagramFile);
        }

        _runConsole.WriteLine("\nFinished running diagrams.");
        return 0;
    }

    public bool IsVerbose = _diagramOptions.Verbose;

    public void RunDiagramFile(string diagramPath)
    {
        string callerFilePath = CurrentDirectory + "/";  // Slash needed for fix of https://github.com/StateSmith/StateSmith/issues/345

        RunnerSettings runnerSettings = new(diagramFile: absolutePath, transpilerId: _diagramOptions.Lang);
        runnerSettings.simulation.enableGeneration = !_diagramOptions.NoSimGen; // enabled by default

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
            return; 
        }

        LoggingCodeFileWriter loggingCodeFileWriter = (LoggingCodeFileWriter)smRunner.GetExperimentalAccess().DiServiceProvider.GetInstanceOf<ICodeFileWriter>();

        smRunner.Run();
    }
}
