#nullable enable

using System.IO;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.SmGraph;
using System.Threading;
using System;

namespace StateSmith.Runner;

/// <summary>
/// This tiny class exists apart from SmRunner so that dependency injection can be used.
/// </summary>
/// // TODO remove this class and use SmRunner directly.
public class SmRunnerInternal
{
    public System.Exception? exception;
    internal bool preDiagramBasedSettingsAlreadyApplied;
    readonly Func<SmRunner> smRunnerProvider; // TODO remove

    public SmRunnerInternal(IConsolePrinter consolePrinter, Func<SmRunner> smRunnerProvider) 
    {
        this.smRunnerProvider = smRunnerProvider;
    }

    public void Run()
    {
        var smRunner = smRunnerProvider();
        var smDesignDescriber = smRunner.smDesignDescriber;
        var outputInfo = smRunner.outputInfo;
        var inputSmBuilder = smRunner.inputSmBuilder;
        var settings = smRunner.context.runnerSettings;
        var consolePrinter = smRunner.consolePrinter;
        var codeGenRunner = smRunner.codeGenRunnerProvider();

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
                var simWebGenerator = smRunner.simWebGeneratorProvider();
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
            smRunner.OutputExceptionDetail(e);

            if (settings.propagateExceptions)
            {
                throw;
            }
        }

        consolePrinter.WriteLine();
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
        // TODO remove
        var smRunner = smRunnerProvider();
        var settings = smRunner.context.runnerSettings;
        var consolePrinter = smRunner.consolePrinter;
        var filePathPrinter = smRunner.filePathPrinter;

        string filePath = settings.DiagramPath;
        filePath = filePathPrinter.PrintPath(filePath);

        consolePrinter.OutputStageMessage($"Compiling file: `{filePath}` "
            + ((settings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{settings.stateMachineName}`")
            + "."
        );
    }


}
