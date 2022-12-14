using StateSmith.Compiling;
using System.Linq;
using Xunit;

namespace StateSmithTest.PseudoStateTests
{
    public class EntryPointValidationTests : EntryInitialValidationTestHelper
    {
        public EntryPointValidationTests()
        {
            s1.AddTransitionTo(s2).viaEntry = "1"; // to make entry point validation successfull
        }

        override protected EntryPoint CreateS2PseudoState()
        {
            return new EntryPoint("1");
        }
    }

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
