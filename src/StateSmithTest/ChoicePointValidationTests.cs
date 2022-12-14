using StateSmith.Compiling;

namespace StateSmithTest.PseudoStateTests
{
    public class ChoicePointValidationTests : EntryInitialValidationTestHelper
    {
        public ChoicePointValidationTests()
        {
            s1.AddTransitionTo(s2_pseudoState);
        }

        override protected ChoicePoint CreateS2PseudoState()
        {
            return new ChoicePoint("1");
        }
    }
}
