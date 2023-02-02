using StateSmith.Output.C99BalancedCoder1;
using System;
using StateSmith.SmGraph;
using System.IO;
using StateSmith.Common;
using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
public class SmRunner
{
    readonly RunnerSettings settings;
    readonly SsServiceProvider sp;
    InputSmBuilder inputSmBuilder;
    ExceptionPrinter exceptionPrinter = new();

    public SmRunner(RunnerSettings settings)
    {
        this.settings = settings;
        sp = new SsServiceProvider(postConfigAction: (context, services) =>
        {
            services.AddSingleton(settings.drawIoSettings);
            services.AddSingleton(settings.mangler);
            services.AddSingleton(settings.style);
            services.AddSingleton(settings.renderConfig);
            services.AddSingleton(settings); // todo_low - split settings up more
            services.AddSingleton<IExpansionVarsPathProvider, CExpansionVarsPathProvider>();
        });

        inputSmBuilder = new(sp);
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior
    /// </summary>
    public SmTransformer SmTransformer => inputSmBuilder.transformer;

    public void Run()
    {
        try
        {
            System.Console.WriteLine();
            RunInputSmBuilder();
            RunCodeGen();
            OutputStageMessage("Finished normally.");
        }
        catch (System.Exception e)
        {
            if (settings.propagateExceptions)
            {
                throw;
            }

            Environment.ExitCode = -1; // lets calling process know that code gen failed

            exceptionPrinter.PrintException(e);
            MaybeDumpErrorDetailsToFile(e);
            OutputStageMessage("Finished with failure.");
        }

        System.Console.WriteLine();
    }

    private void RunInputSmBuilder()
    {
        OutputCompilingDiagramMessage();
        inputSmBuilder.ConvertDiagramFileToSmVertices(settings.diagramFile);
        FindStateMachine();
        inputSmBuilder.FinishRunningCompiler();

        StateMachine sm = inputSmBuilder.Sm.ThrowIfNull();
        sp.SmGetter = () => sm;
    }

    protected void RunCodeGen()
    {
        CodeGenRunner codeGenRunner = sp.GetServiceOrCreateInstance();
        codeGenRunner.Run();
    }

    private void FindStateMachine()
    {
        if (settings.stateMachineName != null)
        {
            inputSmBuilder.FindStateMachineByName(settings.stateMachineName);
        }
        else
        {
            inputSmBuilder.FindSingleStateMachine();
        }

        OutputStageMessage($"State machine `{inputSmBuilder.Sm!.Name}` selected.");
    }

    private void OutputCompilingDiagramMessage()
    {
        // https://github.com/StateSmith/StateSmith/issues/79
        string filePath = settings.diagramFile;
        if (settings.filePathPrintBase.Trim().Length > 0)
        {
            filePath = Path.GetRelativePath(settings.filePathPrintBase, settings.diagramFile);
        }
        filePath = filePath.Replace('\\', '/');

        OutputStageMessage($"Compiling file: `{filePath}` "
            + ((settings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{settings.stateMachineName}`")
            + "."
        );
    }

    protected static void OutputStageMessage(string message)
    {
        // todo_low add logger functionality
        Console.WriteLine("StateSmith Runner - " + message);
    }

    // https://github.com/StateSmith/StateSmith/issues/82
    private void MaybeDumpErrorDetailsToFile(Exception e)
    {
        if (!settings.dumpErrorsToFile)
        {
            Console.Error.WriteLine($"You can enable exception detail dumping by setting `{nameof(RunnerSettings)}.{nameof(RunnerSettings.dumpErrorsToFile)}` to true.");
            return;
        }

        var errorDetailFilePath = settings.diagramFile + ".err.txt";
        errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
        exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
        Console.Error.WriteLine("Additional exception detail dumped to file: " + errorDetailFilePath);
    }
}
