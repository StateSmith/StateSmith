using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.IO;

#nullable enable

namespace StateSmith.Output;

public class GilAlgoCodeGen : ICodeGenRunner
{
    protected readonly IGilAlgo gilAlgo;
    protected readonly IGilTranspiler transpiler;
    protected readonly IStateMachineProvider stateMachineProvider; // todo - pass to Run() instead.
    protected readonly ExpansionsPrep expansionsPrep;
    protected readonly IOutputInfo outputInfo;
    protected readonly RunnerSettings runnerSettings;
    protected readonly IConsolePrinter consolePrinter;

    public GilAlgoCodeGen(IGilAlgo gilAlgo, IGilTranspiler transpiler, IStateMachineProvider stateMachineProvider, ExpansionsPrep expansionsPrep, IOutputInfo outputInfo, RunnerSettings runnerSettings, IConsolePrinter consolePrinter)
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

            if (runnerSettings.outputGilCodeAlways)
                DumpGilCode(sm, gilCode);
        }
        catch (Exception e)
        {
            if (runnerSettings.dumpGilCodeOnError)
            {
                // get the most accurate GIL code if it is available in case other modifications were made
                if (e is TranspilerException transpilerException && transpilerException.GilCode != null)
                {
                    gilCode = transpilerException.GilCode;
                }

                DumpGilCode(sm, gilCode);
            }
            else
            {
                consolePrinter.WriteErrorLine($"You can enable exception detail dumping by setting `{nameof(RunnerSettings.dumpGilCodeOnError)}` to true.");
            }

            throw new TranspilerException($"Failed transpiling Generic Intermediate Language (GIL) code with transpiler: {transpiler.GetType()}", e);
        }
    }

    private void DumpGilCode(StateMachine sm, string gilCode)
    {
        string gilOutputPath = $"{outputInfo.OutputDirectory}{sm.Name}.gil.cs.txt";

        File.WriteAllText(gilOutputPath, gilCode);
        consolePrinter.WriteLine($"You can inspect the generated Generic Intermediate Language (GIL) code here: {gilOutputPath}");
    }
}
