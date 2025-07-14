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
    readonly Func<SmRunner> smRunnerProvider;
    readonly SimWebGenerator simWebGenerator;

    public SmRunnerInternal(ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter, InputSmBuilder inputSmBuilder, RunnerSettings settings, Func<SmRunner> smRunnerProvider, ICodeGenRunner codeGenRunner, FilePathPrinter filePathPrinter, SimWebGenerator simWebGenerator)
    {
        this.smRunnerProvider = smRunnerProvider;
        this.inputSmBuilder = inputSmBuilder;
        this.settings = settings;
        this.codeGenRunner = codeGenRunner;
        this.exceptionPrinter = exceptionPrinter; // TODO one of these two causes a circular dependency
        this.consolePrinter = consolePrinter; // TODO one of these two causes a circular dependency
        this.filePathPrinter = filePathPrinter;
        this.simWebGenerator = simWebGenerator;

    }

    public void Run()
    {
        var smRunner = smRunnerProvider();
        var smDesignDescriber = smRunner.smDesignDescriber;
        var outputInfo = smRunner.outputInfo;

        // TODO better way to do this?
        SmRunner.AppUseDecimalPeriod();   // done here as well to help with unit tests

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

            if (settings.simulation.enableGeneration)
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


}
