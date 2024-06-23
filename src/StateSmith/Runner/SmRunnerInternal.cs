#nullable enable

using System.IO;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.SmGraph;
using System.Globalization;
using System.Threading;
using StateSmith.Output.Sim;

namespace StateSmith.Runner;

/// <summary>
/// This tiny class exists apart from SmRunner so that dependency injection can be used.
/// </summary>
public class SmRunnerInternal
{
    public System.Exception? exception;
    internal bool preDiagramBasedSettingsAlreadyApplied;
    readonly InputSmBuilder inputSmBuilder;
    readonly RunnerSettings settings;
    readonly ICodeGenRunner codeGenRunner;
    readonly ExceptionPrinter exceptionPrinter;
    readonly IConsolePrinter consolePrinter;
    readonly FilePathPrinter filePathPrinter;
    readonly SmDesignDescriber smDesignDescriber;
    readonly OutputInfo outputInfo;
    readonly SimWebGenerator simWebGenerator;

    public SmRunnerInternal(InputSmBuilder inputSmBuilder, RunnerSettings settings, ICodeGenRunner codeGenRunner, ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter, FilePathPrinter filePathPrinter, SmDesignDescriber smDesignDescriber, OutputInfo outputInfo, SimWebGenerator simWebGenerator)
    {
        this.inputSmBuilder = inputSmBuilder;
        this.settings = settings;
        this.codeGenRunner = codeGenRunner;
        this.exceptionPrinter = exceptionPrinter;
        this.consolePrinter = consolePrinter;
        this.filePathPrinter = filePathPrinter;
        this.smDesignDescriber = smDesignDescriber;
        this.outputInfo = outputInfo;
        this.simWebGenerator = simWebGenerator;
    }

    public void Run()
    {
        AppUseDecimalPeriod();   // done here as well to help with unit tests

        try
        {
            if (preDiagramBasedSettingsAlreadyApplied)
            {
                // we need to prevent diagram settings from being applied twice
                DiagramBasedSettingsPreventer.Process(inputSmBuilder.transformer);
            }

            consolePrinter.WriteLine();
            OutputCompilingDiagramMessage();

            var sm = SetupAndFindStateMachine(inputSmBuilder, settings);
            outputInfo.baseFileName = sm.Name;

            consolePrinter.OutputStageMessage($"State machine `{sm.Name}` selected.");
            smDesignDescriber.Prepare();
            smDesignDescriber.DescribeBeforeTransformations();

            inputSmBuilder.FinishRunning();
            smDesignDescriber.DescribeAfterTransformations();
            codeGenRunner.Run();

            if (settings.simulation.enableGeneration)
            {
                simWebGenerator.RunnerSettings.propagateExceptions = settings.propagateExceptions;
                simWebGenerator.RunnerSettings.outputStateSmithVersionInfo = settings.outputStateSmithVersionInfo;
                simWebGenerator.Generate(diagramPath: settings.DiagramPath, outputDir: settings.simulation.outputDirectory.ThrowIfNull());
            }

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

    internal static StateMachine SetupAndFindStateMachine(InputSmBuilder inputSmBuilder, RunnerSettings settings)
    {
        // If the inputSmBuilder already has a state machine, then use it.
        // Used by test code.
        // Might also be used in future to allow compiling plantuml without a diagram file.
        if (inputSmBuilder.HasStateMachine)
        {
            return inputSmBuilder.GetStateMachine();
        }

        inputSmBuilder.ConvertDiagramFileToSmVertices(settings.DiagramPath);

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

        var errorDetailFilePath = settings.DiagramPath + ".err.txt";
        errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
        exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
        consolePrinter.WriteErrorLine("Additional exception detail dumped to file: " + errorDetailFilePath);
    }

    private void OutputCompilingDiagramMessage()
    {
        string filePath = settings.DiagramPath;
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
        settings.DiagramPath = PathUtils.EnsurePathAbsolute(settings.DiagramPath, relativeDirectory);
        
        settings.outputDirectory ??= Path.GetDirectoryName(settings.DiagramPath).ThrowIfNull();
        settings.outputDirectory = ProcessDirPath(settings.outputDirectory, relativeDirectory);

        settings.filePathPrintBase ??= relativeDirectory;
        settings.filePathPrintBase = ProcessDirPath(settings.filePathPrintBase, relativeDirectory);

        settings.smDesignDescriber.outputDirectory ??= settings.outputDirectory;
        settings.smDesignDescriber.outputDirectory = ProcessDirPath(settings.smDesignDescriber.outputDirectory, relativeDirectory);

        if (settings.simulation.enableGeneration)
        {
            settings.simulation.outputDirectory ??= settings.outputDirectory;
            settings.simulation.outputDirectory = ProcessDirPath(settings.simulation.outputDirectory, relativeDirectory);
        }
    }

    private static string ProcessDirPath(string dirPath, string relativeDirectory)
    {
        var resultPath = PathUtils.EnsurePathAbsolute(dirPath, relativeDirectory);
        resultPath = PathUtils.EnsureDirEndingSeperator(resultPath);
        return resultPath;
    }
}
