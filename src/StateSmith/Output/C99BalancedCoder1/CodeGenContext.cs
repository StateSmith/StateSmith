using StateSmith.SmGraph;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using System.Text;
using StateSmith.Runner;

#nullable enable

namespace StateSmith.Output.C99BalancedCoder1;

public class CodeGenContext
{
    public CodeStyleSettings style;
    public CNameMangler mangler;
    public StringBuilder hFileSb = new();
    public StringBuilder cFileSb = new();
    public StateMachine Sm => stateMachineProvider.GetStateMachine();
    public Expander expander;
    public RenderConfigC renderConfig;
    public PseudoStateHandlerBuilder pseudoStateHandlerBuilder = new();

    readonly IStateMachineProvider stateMachineProvider;

    internal CodeGenContext(StateMachine sm)
    {
        stateMachineProvider = new StateMachineProvider(sm);
        mangler = new(sm);
        renderConfig = new();
        expander = new();
        style = new();
    }

    public CodeGenContext(RenderConfigC renderConfig, Expander expander, CNameMangler mangler, CodeStyleSettings style, IStateMachineProvider stateMachineProvider)
    {
        this.renderConfig = renderConfig;
        this.expander = expander;
        this.mangler = mangler;
        this.style = style;
        this.stateMachineProvider = stateMachineProvider;
    }

    public void AddExpansions(UserExpansionScriptBase userObject)
    {
        ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
        expanderFileReflection.AddAllExpansions(userObject);
    }
}
