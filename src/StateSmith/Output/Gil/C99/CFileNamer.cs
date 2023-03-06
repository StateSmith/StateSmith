#nullable enable

using StateSmith.Common;
using StateSmith.Runner;
using StateSmith.SmGraph;

namespace StateSmith.Output.Gil.C99;

public class CFileNamer : ICFileNamer
{
    protected StateMachineProvider stateMachineProvider;
    protected StateMachine Sm => stateMachineProvider.GetStateMachine().ThrowIfNull();

    public CFileNamer(StateMachine sm) : this(new StateMachineProvider(sm)) { }

    public CFileNamer(StateMachineProvider stateMachineProvider)
    {
        this.stateMachineProvider = stateMachineProvider;
    }

    public string MakeHFileName() => $"{Sm.Name}.h";
    public string MakeCFileName() => $"{Sm.Name}.c";
}
