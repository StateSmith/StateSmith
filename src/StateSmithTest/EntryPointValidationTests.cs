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
}
