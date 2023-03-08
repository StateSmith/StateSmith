using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.IO;

#nullable enable

namespace StateSmith.Output;

public class TranspilerException : Exception
{
    public TranspilerException(string? message, Exception? innerException = null) : base(message, innerException)
    {
    }
}

public class GilAlgoCodeGen : ICodeGenRunner
{
    protected readonly IGilAlgo gilAlgo;
    protected readonly IGilTranspiler transpiler;
    protected readonly IStateMachineProvider stateMachineProvider; // todo - pass to Run() instead.
    protected readonly ExpansionsPrep expansionsPrep;
    protected readonly OutputInfo outputInfo;
    protected readonly RunnerSettings runnerSettings;
    protected readonly IConsolePrinter consolePrinter;

    public GilAlgoCodeGen(IGilAlgo gilAlgo, IGilTranspiler transpiler, IStateMachineProvider stateMachineProvider, ExpansionsPrep expansionsPrep, OutputInfo outputInfo, RunnerSettings runnerSettings, IConsolePrinter consolePrinter)
    {
        this.gilAlgo = gilAlgo;
        this.transpiler = transpiler;
        this.stateMachineProvider = stateMachineProvider;
        this.expansionsPrep = expansionsPrep;
        this.outputInfo = outputInfo;
        this.runnerSettings = runnerSettings;
        this.consolePrinter = consolePrinter;
    }

    public void Run()
    {
        expansionsPrep.PrepForCodeGen();

        var sm = stateMachineProvider.GetStateMachine();

        var gilCode = gilAlgo.GenerateGil(sm);

        TryTranspiling(sm, gilCode);
    }

    private void TryTranspiling(StateMachine sm, string gilCode)
    {
        try
        {
            transpiler.TranspileAndOutputCode(gilCode);
        }
        catch (Exception e)
        {
            if (runnerSettings.dumpGilCodeOnError)
            {
                string gilPath = $"{outputInfo.outputDirectory}{sm.Name}.gil.cs";
                File.WriteAllText(gilPath, gilCode);
                consolePrinter.WriteErrorLine($"You can inspect the generated Generic Intermediate Langauge (GIL) code here: {gilPath}");
            }
            else
            {
                consolePrinter.WriteErrorLine($"You can enable exception detail dumping by setting `{nameof(RunnerSettings)}.{nameof(RunnerSettings.dumpGilCodeOnError)}` to true.");
            }

            throw new TranspilerException($"Failed transpiling Generic Intermediate Langauge (GIL) code with transpiler: {transpiler.GetType()}", e);
        }
    }
}
