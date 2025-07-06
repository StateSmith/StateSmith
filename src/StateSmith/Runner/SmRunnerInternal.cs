#nullable enable

using System.IO;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.SmGraph;
using System.Globalization;
using System.Threading;
using StateSmith.Output.Sim;
using System;

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
    readonly SimWebGenerator? simWebGenerator;

    public SmRunnerInternal(InputSmBuilder inputSmBuilder, RunnerSettings settings, ICodeGenRunner codeGenRunner, ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter, FilePathPrinter filePathPrinter, SmDesignDescriber smDesignDescriber, OutputInfo outputInfo, AlgoTranspilerCustomizer algoTranspilerCustomizer, SimWebGenerator? simWebGenerator = null)
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

        algoTranspilerCustomizer.Customize(settings.algorithmId, settings.transpilerId);
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
            consolePrinter.WriteLine("StateSmith lib ver - " + LibVersionInfo.GetVersionInfoString());
            OutputCompilingDiagramMessage();

            var sm = SetupAndFindStateMachine(inputSmBuilder, settings);
            outputInfo.baseFileName = sm.Name;

            consolePrinter.OutputStageMessage($"State machine `{sm.Name}` selected.");
            smDesignDescriber.Prepare();
            smDesignDescriber.DescribeBeforeTransformations();

            inputSmBuilder.FinishRunning();
            smDesignDescriber.DescribeAfterTransformations();
            codeGenRunner.Run();

            if (settings.simulation.enableGeneration && simWebGenerator != null)
            {
                simWebGenerator.RunnerSettings.propagateExceptions = settings.propagateExceptions;
                simWebGenerator.RunnerSettings.outputStateSmithVersionInfo = settings.outputStateSmithVersionInfo;
                simWebGenerator.Generate(diagramPath: settings.DiagramPath, outputDir: settings.simulation.outputDirectory.ThrowIfNull());
            }

            consolePrinter.OutputStageMessage("Finished normally.");
        }
        catch (Exception e)
        {
            exception = e;

            // print error detail before rethrowing https://github.com/StateSmith/StateSmith/issues/375
            OutputExceptionDetail(e);

            if (settings.propagateExceptions)
            {
                throw;
            }
        }

        consolePrinter.WriteLine();
    }

    public void OutputExceptionDetail(Exception e)
    {
        exceptionPrinter.PrintException(e);

        consolePrinter.WriteErrorLine($"Related error info/debug settings: 'dumpErrorsToFile', 'propagateExceptions'. See https://github.com/StateSmith/StateSmith/blob/main/docs/settings.md .");

        // https://github.com/StateSmith/StateSmith/issues/82
        if (settings.dumpErrorsToFile)
        {
            var errorDetailFilePath = settings.DiagramPath + ".err.txt";
            errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
            exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
            consolePrinter.WriteErrorLine("Exception details dumped to file: " + errorDetailFilePath);
        }

        consolePrinter.OutputStageMessage("Finished with failure.");

        // Give stdout a chance to print the exception before exception is thrown.
        // We sometimes would see our printed detail message info cut off by dotnet exception handling.
        // See https://github.com/StateSmith/StateSmith/issues/375
        Thread.Sleep(100);
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
        var tmp = settings.DiagramPath;
        settings.DiagramPath = PathUtils.EnsurePathAbsolute(settings.DiagramPath, relativeDirectory);
        Console.WriteLine($"BOOGA SmRunnerInternal.ResolveFilePaths callingFilePath({callingFilePath}) before({tmp}) after({settings.DiagramPath})");
        // throw new ArgumentException();

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
