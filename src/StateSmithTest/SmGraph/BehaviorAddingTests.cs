using FluentAssertions;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.SmGraph;

public class BehaviorAddingTests
{
    readonly State state = new("SOME_STATE");

    public BehaviorAddingTests()
    {
        state.AddBehavior(new Behavior(trigger: "EV1"));
        state.AddBehavior(new Behavior(trigger: "EV2"));

        state.ShouldHaveUmlBehaviors("""
            EV1
            EV2
            """);
    }

    [Fact]
    public void InitiallyNone()
    {
        State blankState = new("BLANK_STATE");
        blankState._behaviors.Should().HaveCount(0);
    }

    [Fact]
    public void AddBehaviorAtStart()
    {
        state.AddBehavior(new Behavior(trigger: "NEW_EV"), index: 0);

        state.ShouldHaveUmlBehaviors("""
            NEW_EV
            EV1
            EV2
            """);
    }

    [Fact]
    public void AddBehaviorInMid()
    {
        state.AddBehavior(new Behavior(trigger: "NEW_EV"), index: 1);

        state.ShouldHaveUmlBehaviors("""
            EV1
            NEW_EV
            EV2
            """);
    }

    [Fact]
    public void AddBehaviorAtEnd()
    {
        state.AddBehavior(new Behavior(trigger: "NEW_EV"), index: 2);

        state.ShouldHaveUmlBehaviors("""
            EV1
            EV2
            NEW_EV
            """);
    }

    [Fact]
    public void AddBehaviorAtIndexWayTooBig()
    {
        state.AddBehavior(new Behavior(trigger: "NEW_EV"), index: 100);

        state.ShouldHaveUmlBehaviors("""
            EV1
            EV2
            NEW_EV
            """);
    }

    [Fact]
    public void AddEnterActionDefaultAtEnd()
    {
        state.AddEnterAction("my_enter_action1();");

        state.ShouldHaveUmlBehaviors("""
            EV1
            EV2
            enter / { my_enter_action1(); }
            """);
    }

    [Fact]
    public void AddEnterActionAtIndex()
    {
        state.AddEnterAction("my_enter_action1();", index: 0);

        state.ShouldHaveUmlBehaviors("""
            enter / { my_enter_action1(); }
            EV1
            EV2
            """);
    }

    [Fact]
    public void AddExitActionDefaultAtEnd()
    {
        state.AddExitAction("my_exit_action1();");

        state.ShouldHaveUmlBehaviors("""
            EV1
            EV2
            exit / { my_exit_action1(); }
            """);
    }

    [Fact]
    public void AddExitActionAtIndex()
    {
        state.AddExitAction("my_exit_action1();", index: 0);

        state.ShouldHaveUmlBehaviors("""
            exit / { my_exit_action1(); }
            EV1
            EV2
            """);
    }
}
