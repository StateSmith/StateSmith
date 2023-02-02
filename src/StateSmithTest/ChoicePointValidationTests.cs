using StateSmith.SmGraph;

namespace StateSmithTest.PseudoStateTests
{
    public class ChoicePointValidationTests : PseudoStateValidationTestHelper
    {
        public ChoicePointValidationTests()
        {
            AddBlankS2PseudoStateTransition();
            s1.AddTransitionTo(s2_pseudoState);
        }

        protected override void AddBlankS2PseudoStateTransition()
        {
            s2_pseudoState.AddTransitionTo(s2_1);
        }

        override protected ChoicePoint CreateS2PseudoState()
        {
            return new ChoicePoint("1");
        }
    }
}
