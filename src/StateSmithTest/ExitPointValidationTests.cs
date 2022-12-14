using StateSmith.Compiling;

namespace StateSmithTest.PseudoStateTests
{
    public class ExitPointValidationTests : PseudoStateValidationTestHelper
    {
        public ExitPointValidationTests()
        {
            s2_1.AddTransitionTo(s2_pseudoState);
            AddBlankS2PseudoStateTransition(); // to make exit point validation successfull
        }

        protected override void AddBlankS2PseudoStateTransition()
        {
            s2_pseudoState.AddTransitionTo(s1);
        }

        override protected ExitPoint CreateS2PseudoState()
        {
            return new ExitPoint("1");
        }
    }
}
