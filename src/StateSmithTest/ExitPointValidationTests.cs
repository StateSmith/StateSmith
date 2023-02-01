using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.PseudoStateTests
{
    public class ExitPointValidationTests : PseudoStateValidationTestHelper
    {
        public ExitPointValidationTests()
        {
            s2_1.AddTransitionTo(s2_pseudoState);
        }

        protected override void AddBlankS2PseudoStateTransition()
        {
            s2_pseudoState.AddTransitionTo(s1);
        }

        override protected ExitPoint CreateS2PseudoState()
        {
            var exitPoint = new ExitPoint("1");
            exitPoint.AddTransitionTo(s1);
            return exitPoint;
        }

        [Fact]
        public void SameNameOkIfNotInSameScope()
        {
            var duplicateNamed = new ExitPoint("1");
            s2_1.AddChild(duplicateNamed);
            duplicateNamed.AddTransitionTo(s2_1).guardCode = "true"; // to get around yEd concern
            s2_1_1.AddTransitionTo(duplicateNamed);
            RunCompiler();
        }

        [Fact]
        public void ErrorIfDuplicateNameInParent()
        {
            var duplicateNamed = new ExitPoint("1");
            s2.AddChild(duplicateNamed);
            duplicateNamed.AddTransitionTo(s2_1);
            s2_1.AddTransitionTo(duplicateNamed);
            ExpectVertexValidationExceptionWildcard("*duplicate*");
        }
    }
}
