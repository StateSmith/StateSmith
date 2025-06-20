using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.Runner;

namespace StateSmithTest;

public class EventGatheringTests
{
    private static StateMachine BuildTestGraph(string rootName)
    {
        var sm = new StateMachine(name: rootName);

        var s1 = sm.AddChild(new State(name: "s1"));
        s1.AddChild(new State(name: "s1_1"));

        var initialStateVertex = sm.AddChild(new InitialState());
        initialStateVertex.AddTransitionTo(s1);

        return sm;
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/121
    /// </summary>
    [Fact]
    public void TestOnlyInitialTransition()
    {
        var sm = BuildTestGraph("Sm1");
        InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();
        inputSmBuilder.SetStateMachineRoot(sm);
        inputSmBuilder.FinishRunning();

        sm.GetEventListCopy().Should().BeEquivalentTo("do");
    }

    [Fact]
    public void Test()
    {
        var sm = BuildTestGraph("Sm1");
        InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();
        inputSmBuilder.SetStateMachineRoot(sm);
        var map = new NamedVertexMap(sm);
        State GetState(string stateName) => map.GetState(stateName);

        GetState("s1").AddBehavior(new Behavior()
        {
            _triggers = new List<string>() { "EV1", "do" }
        });
        GetState("s1_1").AddBehavior(new Behavior()
        {
            _triggers = new List<string>() { "enter", "exit", "ZIP" }
        });

        inputSmBuilder.FinishRunning();

        sm.GetEventListCopy().Should().BeEquivalentTo("do", "ev1", "zip");
    }

    [Fact]
    public void Test2()
    {
        var sm = BuildTestGraph("Sm2");
        InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();
        inputSmBuilder.SetStateMachineRoot(sm);
        var map = new NamedVertexMap(sm);
        State GetState(string stateName) => map.GetState(stateName);

        GetState("s1").AddBehavior(new Behavior()
        {
            _triggers = new List<string>() { "SM2_EV1", "do", "exit" }
        });
        GetState("s1_1").AddBehavior(new Behavior()
        {
            _triggers = new List<string>() { "enter", "exit", "SM2_ZIP" }
        });

        inputSmBuilder.FinishRunning();

        sm.GetEventListCopy().Should().BeEquivalentTo("do", "sm2_ev1", "sm2_zip");
    }
}
