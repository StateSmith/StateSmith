using StateSmith.Input.Expansions;
using StateSmith.Output.C99BalancedCoder1;
using StateSmith.SmGraph;

#nullable enable

namespace StateSmith.Output;

internal class DynamicVarsResolver
{
    readonly CNameMangler mangler;
    readonly Expander expander;
    readonly IExpansionVarsPathProvider expansionVarsPathProvider;
    readonly StateMachine sm;

    public DynamicVarsResolver(CNameMangler mangler, Expander expander, IExpansionVarsPathProvider expansionVarsPathProvider, StateMachine sm)
    {
        this.mangler = mangler;
        this.expander = expander;
        this.expansionVarsPathProvider = expansionVarsPathProvider;
        this.sm = sm;
    }

    public void Resolve()
    {
        foreach (var h in sm.historyStates)
        {
            string actualVarName = mangler.HistoryVarName(h);
            expander.AddVariableExpansion(h.stateTrackingVarName, expansionVarsPathProvider.ExpansionVarsPath + actualVarName);
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
                sm.variables += $"enum {mangler.HistoryVarEnumName(h)} {actualVarName};\n";
            }
        }
    }
}
