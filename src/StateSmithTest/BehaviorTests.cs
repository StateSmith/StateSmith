using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;

namespace StateSmithTest;

public class BehaviorTests
{
    [Fact]
    public void DescribeAsUml()
    {
        var b = new Behavior();
        b.DescribeAsUml().Should().Be("");
    }

    [Fact]
    public void DescribeAsUml_WithEverythingSimple()
    {
        var root = new State("Root");
        var s1 = new State("s1");
        root.AddChild(s1);

        var b = new Behavior();
        b.order = 1.25;
        b._triggers.Add("EV1");
        b.guardCode = "x > 10";
        b.actionCode = "x = 0;";
        b.viaEntry = "entry1";
        b.viaExit = "exit1";

        b._transitionTarget = s1;

        b.DescribeAsUml().Should().Be("1.25. EV1 [x > 10] / { x = 0; } via exit exit1 via entry entry1 TransitionTo(s1)");
    }

    [Fact]
    public void DescribeAsUml_Else()
    {
        var root = new State("Root");
        var s1 = new State("s1");
        root.AddChild(s1);

        var b = new Behavior();
        b.order = Behavior.ELSE_ORDER;
        b.actionCode = "x = 0;";

        b._transitionTarget = s1;

        b.DescribeAsUml().Should().Be("else / { x = 0; } TransitionTo(s1)");
    }

    [Fact]
    public void DescribeAsUml_MultilineGuard()
    {
        var b = new Behavior();
        b.guardCode = "g1() && \ng2()";
        b.DescribeAsUml().Should().Be("[g1() && \\ng2()]");
    }

    [Fact]
    public void DescribeAsUml_MultilineAction()
    {
        var b = new Behavior();
        b.actionCode = "a1();\na2();";
        b.DescribeAsUml().Should().Be("/ { a1();\\na2(); }");
    }

    [Fact]
    public void DescribeAsUml_MultipleTriggers()
    {
        var b = new Behavior();
        b._triggers.Add("enter");
        b._triggers.Add("do");
        b._triggers.Add("exit");
        b.DescribeAsUml().Should().Be("(enter, do, exit)");
    }

    [Fact]
    public void DescribeAsUml_MultipleTriggers2()
    {
        var b = new Behavior();
        b._triggers.Add("enter");
        b._triggers.Add("do");
        b._triggers.Add("exit");
        b.guardCode = "ready";
        b.actionCode = "get_some();";
        b.DescribeAsUml().Should().Be("(enter, do, exit) [ready] / { get_some(); }");
    }
}
