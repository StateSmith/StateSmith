using FluentAssertions;
using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.Runner;
using System.Collections.Generic;
using System.Linq;
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
    public override void MultipleBehavior()
    {
        AddBlankS2PseudoStateTransition();
        ExpectVertexValidationExceptionWildcard("* a single default transition. Found *2*");

        AddBlankS2PseudoStateTransition();
        ExpectVertexValidationExceptionWildcard("* a single default transition. Found *3*");
    }

    [Fact]
    public override void GuardWithDefaultTransition()
    {
        s2_pseudoState.Behaviors.Single().guardCode = "x > 100";
        AddBlankS2PseudoStateTransition(); // default no-guard transition
        ExpectVertexValidationExceptionWildcard("* a single default transition. Found *2*");
    }

    [Fact]
    public void TooManySiblings()
    {
        s2.AddChild(new HistoryVertex());
        ExpectVertexValidationExceptionWildcard("* 1 history* allowed*. Found *2*");
        s2.AddChild(new HistoryVertex());
        ExpectVertexValidationExceptionWildcard("* 1 history* allowed*. Found *3*");
    }
}
