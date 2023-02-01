using StateSmith.SmGraph;
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

        [Fact]
        public void SameNameOkIfNotInSameScope()
        {
            var duplicateNamed = new EntryPoint("1");
            s2_1.AddChild(duplicateNamed);
            duplicateNamed.AddTransitionTo(s2_1_1);
            RunCompiler();
        }

        [Fact]
        public void ErrorIfDuplicateNameInParent()
        {
            var duplicateNamed = new EntryPoint("1");
            s2.AddChild(duplicateNamed);
            duplicateNamed.AddTransitionTo(s2_1);
            ExpectVertexValidationExceptionWildcard("*duplicate*");
        }
    }
}
