#nullable enable

using System.IO;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.SmGraph;
using System.Globalization;
using System.Threading;

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
    readonly SmDesignDescriber smDesignDescriber;
    readonly OutputInfo outputInfo;

    public SmRunnerInternal(InputSmBuilder inputSmBuilder, RunnerSettings settings, ICodeGenRunner codeGenRunner, ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter, FilePathPrinter filePathPrinter, SmDesignDescriber smDesignDescriber, OutputInfo outputInfo)
    {
        this.inputSmBuilder = inputSmBuilder;
        this.settings = settings;
        this.codeGenRunner = codeGenRunner;
        this.exceptionPrinter = exceptionPrinter;
        this.consolePrinter = consolePrinter;
        this.filePathPrinter = filePathPrinter;
        this.smDesignDescriber = smDesignDescriber;
        this.outputInfo = outputInfo;
    }

    public void Run()
    {
        AppUseDecimalPeriod();   // done here as well to help with unit tests

        try
        {
            consolePrinter.WriteLine();
            OutputCompilingDiagramMessage();

            var sm = SetupAndFindStateMachine();
            outputInfo.baseFileName = sm.Name;

            consolePrinter.OutputStageMessage($"State machine `{sm.Name}` selected.");
            smDesignDescriber.Setup();
            smDesignDescriber.DescribeBeforeTransformations();

            inputSmBuilder.FinishRunning();
            smDesignDescriber.DescribeAfterTransformations();
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

    /// <summary>
    /// Force application number parsing to use periods for decimal points instead of commas.
    /// Fix for https://github.com/StateSmith/StateSmith/issues/159
    /// </summary>
    public static void AppUseDecimalPeriod()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    }

    public static void ResolveFilePaths(RunnerSettings settings, string? callingFilePath)
    {
        var relativeDirectory = Path.GetDirectoryName(callingFilePath).ThrowIfNull();
        settings.diagramFile = PathUtils.EnsurePathAbsolute(settings.diagramFile, relativeDirectory);
        
        settings.outputDirectory ??= Path.GetDirectoryName(settings.diagramFile).ThrowIfNull();
        settings.outputDirectory = ProcessDirPath(settings.outputDirectory, relativeDirectory);

        settings.filePathPrintBase ??= relativeDirectory;
        settings.filePathPrintBase = ProcessDirPath(settings.filePathPrintBase, relativeDirectory);

        settings.smDesignDescriber.outputDirectory ??= settings.outputDirectory;
        settings.smDesignDescriber.outputDirectory = ProcessDirPath(settings.smDesignDescriber.outputDirectory, relativeDirectory);
    }

    private static string ProcessDirPath(string dirPath, string relativeDirectory)
    {
        var resultPath = PathUtils.EnsurePathAbsolute(dirPath, relativeDirectory);
        resultPath = PathUtils.EnsureDirEndingSeperator(resultPath);
        return resultPath;
    }
}
