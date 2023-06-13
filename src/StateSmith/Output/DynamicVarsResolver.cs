using StateSmith.Output.Algos.Balanced1;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Validation;

#nullable enable

namespace StateSmith.Output;

public class DynamicVarsResolver
{
    readonly NameMangler mangler;
    readonly IStateMachineProvider stateMachineProvider;

    public DynamicVarsResolver(NameMangler mangler, IStateMachineProvider stateMachineProvider)
    {
        this.mangler = mangler;
        this.stateMachineProvider = stateMachineProvider;
    }

    public void Resolve()
    {
        var sm = stateMachineProvider.GetStateMachine();

        foreach (var h in sm.historyStates)
        {
            string actualVarName = h.stateTrackingVarName;
            bool useU8 = false;

            if (useU8)
            {
                sm.variables += $"uint8_t {actualVarName};\n";
                if (h.Behaviors.Count > 255)
                {
                    throw new VertexValidationException(h, "can't support more than 255 tracked history states right now."); //uint8_t limitation.
                }
            }
            else
            {
                sm.variables += $"{mangler.HistoryVarEnumType(h)} {actualVarName};\n";
            }
        }
    }
}
