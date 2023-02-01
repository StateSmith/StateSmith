using System;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;

namespace StateSmithTest.PseudoStateTests
{
    public class InitialStateValidationTests : EntryInitialValidationTestHelper
    {
        override protected InitialState CreateS2PseudoState()
        {
            return new InitialState();
        }

        [Fact]
        public void StateMachineMustHaveExactly1()
        {
            sm.RemoveChild(rootInitialState);
            ExpectVertexValidationExceptionWildcard("State machines must have exactly 1 initial state. Actual count: 0.");

            sm.AddChild(new InitialState());
            sm.AddChild(new InitialState());
            ExpectVertexValidationExceptionWildcard("State machines must have exactly 1 initial state. Actual count: 2.");
        }

        [Fact]
        public void TooManySiblings()
        {
            s2.AddChild(new InitialState());
            ExpectVertexValidationExceptionWildcard("*can only have a single initial state*2*");

            s2.AddChild(new InitialState());
            ExpectVertexValidationExceptionWildcard("*can only have a single initial state*3*");
        }
    }
}
