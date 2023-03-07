using StateSmith.Runner;

#nullable enable

namespace StateSmith.Output;

public class GilAlgoCodeGen : ICodeGenRunner
{
    protected readonly IGilAlgo gilAlgo;
    protected readonly IGilTranspiler transpiler;
    protected readonly IStateMachineProvider stateMachineProvider; // todo - pass to Run() instead.
    protected readonly ExpansionsPrep expansionsPrep;

    public GilAlgoCodeGen(IGilAlgo gilAlgo, IGilTranspiler transpiler, IStateMachineProvider stateMachineProvider, ExpansionsPrep expansionsPrep)
    {
        this.gilAlgo = gilAlgo;
        this.transpiler = transpiler;
        this.stateMachineProvider = stateMachineProvider;
        this.expansionsPrep = expansionsPrep;
    }

    public void Run()
    {
        expansionsPrep.PrepForCodeGen();

        var sm = stateMachineProvider.GetStateMachine();

        var gilCode = gilAlgo.GenerateGil(sm);
        transpiler.TranspileAndOutputCode(gilCode);
    }
}
