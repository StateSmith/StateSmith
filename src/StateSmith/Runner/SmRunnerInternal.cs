using System.IO;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.SmGraph;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// This tiny class exists apart from SmRunner so that dependency injection can be used.
/// </summary>
public class SmRunnerInternal
{
    public System.Exception? exception;

    readonly InputSmBuilder inputSmBuilder;
    readonly RunnerSettings settings;
    readonly ICodeGenRunner codeGenRunner;
    readonly ExceptionPrinter exceptionPrinter;
    readonly IConsolePrinter consolePrinter;
    readonly FilePathPrinter filePathPrinter;

    public SmRunnerInternal(InputSmBuilder inputSmBuilder, RunnerSettings settings, ICodeGenRunner codeGenRunner, ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter, FilePathPrinter filePathPrinter)
    {
        this.inputSmBuilder = inputSmBuilder;
        this.settings = settings;
        this.codeGenRunner = codeGenRunner;
        this.exceptionPrinter = exceptionPrinter;
        this.consolePrinter = consolePrinter;
        this.filePathPrinter = filePathPrinter;
    }

    public void Run()
    {
        try
        {
            consolePrinter.WriteLine();
            OutputCompilingDiagramMessage();

            var sm = SetupAndFindStateMachine();
            consolePrinter.OutputStageMessage($"State machine `{sm.Name}` selected.");

            inputSmBuilder.FinishRunning();
            codeGenRunner.Run();
            consolePrinter.OutputStageMessage("Finished normally.");
        }
        catch (System.Exception e)
        {
            if (settings.propagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }

        consolePrinter.WriteLine();
    }

    private void HandleException(System.Exception e)
    {
        exception = e;

        exceptionPrinter.PrintException(e);
        MaybeDumpErrorDetailsToFile(e);
        consolePrinter.OutputStageMessage("Finished with failure.");
    }

    private StateMachine SetupAndFindStateMachine()
    {
        inputSmBuilder.ThrowIfNull().ConvertDiagramFileToSmVertices(settings.diagramFile);

        if (settings.stateMachineName != null)
        {
            inputSmBuilder.FindStateMachineByName(settings.stateMachineName);
        }
        else
        {
            inputSmBuilder.FindSingleStateMachine();
        }

        return inputSmBuilder.GetStateMachine();
    }

    // https://github.com/StateSmith/StateSmith/issues/82
    private void MaybeDumpErrorDetailsToFile(System.Exception e)
    {
        if (!settings.dumpErrorsToFile)
        {
            consolePrinter.WriteErrorLine($"You can enable exception detail dumping by setting `{nameof(RunnerSettings)}.{nameof(RunnerSettings.dumpErrorsToFile)}` to true.");
            return;
        }

        var errorDetailFilePath = settings.diagramFile + ".err.txt";
        errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
        exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
        consolePrinter.WriteErrorLine("Additional exception detail dumped to file: " + errorDetailFilePath);
    }

    private void OutputCompilingDiagramMessage()
    {
        string filePath = settings.diagramFile;
        filePath = filePathPrinter.PrintPath(filePath);

        consolePrinter.OutputStageMessage($"Compiling file: `{filePath}` "
            + ((settings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{settings.stateMachineName}`")
            + "."
        );
    }

    public static void ResolveFilePaths(RunnerSettings settings, string? callingFilePath)
    {
        var relativeDirectory = Path.GetDirectoryName(callingFilePath).ThrowIfNull();
        settings.diagramFile = PathUtils.EnsurePathAbsolute(settings.diagramFile, relativeDirectory);
        settings.outputDirectory ??= Path.GetDirectoryName(settings.diagramFile).ThrowIfNull();
        settings.filePathPrintBase ??= relativeDirectory;

        settings.outputDirectory = ProcessDirPath(settings.outputDirectory, relativeDirectory);
        settings.filePathPrintBase = ProcessDirPath(settings.filePathPrintBase, relativeDirectory);
    }

    private static string ProcessDirPath(string dirPath, string relativeDirectory)
    {
        var resultPath = PathUtils.EnsurePathAbsolute(dirPath, relativeDirectory);
        resultPath = PathUtils.EnsureDirEndingSeperator(resultPath);
        return resultPath;
    }
}
