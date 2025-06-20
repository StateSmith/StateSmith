#nullable enable

using Xunit;
using StateSmith.Runner;
using StateSmith.SmGraph;
using FluentAssertions;
using System.Linq;

namespace StateSmithTest.SmGraph;

public class NotesTests
{
    readonly InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();

    private void CompileDrawioFile(string relativeFilePath)
    {
        inputSmBuilder.ConvertDrawIoFileNodesToVertices(TestHelper.GetThisDir() + relativeFilePath);
        inputSmBuilder.FinishRunning();
    }

    [Fact]
    public void Test1()
    {
        string filepath = "notes-nested-states.drawio";
        CompileDrawioFile(filepath);
    }

    [Fact]
    public void Test2()
    {
        string filepath = "notes-test.drawio";
        CompileDrawioFile(filepath);

        StateMachine sm = inputSmBuilder.GetStateMachine();
        NamedVertexMap map = new(sm);
        var s1 = map.GetState("S1");
        var s1_1 = map.GetState("S1_1");
        var s2 = map.GetState("S2");

        sm.Children.Count.Should().Be(3, because:"initial state, S1, S2");

        s1.ShouldHaveUmlBehaviors("""
            EV1 TransitionTo(S2)
            """);
        s1.IncomingTransitions.Count().Should().Be(2, because: "initial state transition and from S2");
        
        s1_1.ShouldHaveUmlBehaviors("""
            EV1 TransitionTo(S1_1)
            """);
        s1_1.IncomingTransitions.Count().Should().Be(2, because:"initial transition and self transition");

        s2.ShouldHaveUmlBehaviors("""
            EV2 TransitionTo(S1)
            """);
        s2.IncomingTransitions.Count().Should().Be(1);
    }
}
