using StateSmith.SmGraph;
using StateSmith.Input.Expansions;
using StateSmith.output.UserConfig;
using System.Text;
# nullable enable

namespace StateSmith.output.C99BalancedCoder1
{
    public class CodeGenContext
    {
        public CodeStyleSettings style = new();
        public CNameMangler mangler;
        public StringBuilder hFileSb = new();
        public StringBuilder cFileSb = new();
        public StateMachine sm;
        public Expander expander = new();
        public IRenderConfigC renderConfig;
        public PseudoStateHandlerBuilder pseudoStateHandlerBuilder = new();

        internal CodeGenContext(StateMachine sm)
        {
            this.sm = sm;
            mangler = new(sm);
            renderConfig = new DummyRenderConfigC();
        }

        public CodeGenContext(StateMachine sm, IRenderConfigC renderConfig)
        {
            this.sm = sm;
            mangler = new(sm);
            this.renderConfig = renderConfig;
        }

        public void AddExpansions(UserExpansionScriptBase userObject)
        {
            ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
            expanderFileReflection.AddAllExpansions(userObject);
        }
    }
}
