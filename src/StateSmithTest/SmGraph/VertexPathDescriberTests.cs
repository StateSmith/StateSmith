using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using Xunit;

namespace StateSmithTest.SmGraph;

public class VertexPathDescriberTests
{
    [Fact]
    public void ParentRelative()
    {
        // pseudo states like initial states are relative to their parent
        InitialState initialState = new StateMachine("MySm1").AddChild(new State("POWERING_UP")).AddChild(new InitialState());
        VertexPathDescriber.Describe(initialState).Should().Be("ROOT.POWERING_UP.<InitialState>");
    }

    [Fact]
    public void Regular()
    {
        State state = new StateMachine("MySm1").AddChild(new State("POWERING_UP")).AddChild(new State("WAIT_FOR_PRESS"));
        VertexPathDescriber.Describe(state).Should().Be("ROOT.POWERING_UP.WAIT_FOR_PRESS");
    }
}
