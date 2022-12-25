using FluentAssertions;
using StateSmith.Compiling;
using System;
using Xunit;

namespace StateSmithTest.PseudoStateTests;

public class HistoryValidationTests : PseudoStateValidationTestHelper
{
    public HistoryValidationTests()
    {
        AddBlankS2PseudoStateTransition();
    }

    protected override void AddBlankS2PseudoStateTransition()
    {
        s2_pseudoState.AddTransitionTo(s2_1);
    }

    override protected PseudoStateVertex CreateS2PseudoState()
    {
        return new HistoryVertex();
    }

    [Fact]
    public void TooManySiblings()
    {
        s2.AddChild(new HistoryVertex());
        ExpectVertexValidationExceptionWildcard("*Only 1 history vertex is allowed in a state. Found *2*");
        s2.AddChild(new HistoryVertex());
        ExpectVertexValidationExceptionWildcard("*Only 1 history vertex is allowed in a state. Found *3*");
    }

    [Fact]
    public void HistoryContinue_AtRootLevel()
    {
        sm.AddChild(new HistoryContinueVertex());
        var processor = new HistoryContinueProcessor();

        var vertex = sm;
        Action action = () => processor.Visit(sm);
        action.Should().Throw<VertexValidationException>()
            .WithMessage("wild card message")
            .Where(e => e.vertex == vertex)
            ;
    }
}

