using StateSmith.SmGraph;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using System.Text;
using StateSmith.Runner;

# nullable enable

namespace StateSmith.Output.C99BalancedCoder1
{
    public class CodeGenContext
    {
        public CodeStyleSettings style;
        public CNameMangler mangler;
        public StringBuilder hFileSb = new();
        public StringBuilder cFileSb = new();
        public StateMachine sm;
        public Expander expander;
        public RenderConfigC renderConfig;
        public PseudoStateHandlerBuilder pseudoStateHandlerBuilder = new();

        internal CodeGenContext(StateMachine sm)
        {
            this.sm = sm;
            mangler = new(sm);
            renderConfig = new();
            expander = new();
            style = new();
        }

        public CodeGenContext(RenderConfigC renderConfig, Expander expander, CNameMangler mangler, CodeStyleSettings style, IStateMachineProvider stateMachineProvider)
        {
            this.sm = stateMachineProvider.GetStateMachine();
            this.renderConfig = renderConfig;
            this.expander = expander;
            this.mangler = mangler;
            this.style = style;
        }

        public void AddExpansions(UserExpansionScriptBase userObject)
        {
            ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
            expanderFileReflection.AddAllExpansions(userObject);
        }
    }
}
