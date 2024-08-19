using FluentAssertions;
using StateSmith.Runner;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.Input.DrawIo;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/359
/// </summary>
public class FlatSmRootTest
{
    [Fact]
    public void VertexTest()
    {
        InputSmBuilder builder = new();
        builder.ConvertDrawIoFileNodesToVertices(TestHelper.GetThisDir() + "FlatSmRoot.drawio");
        builder.FindSingleStateMachine();
        var roots = builder.GetRootVertices();
        roots.Should().HaveCount(1);

        var sm = builder.GetStateMachine();
        sm.Children.Should().HaveCount(4);

        var initial = sm.ChildType<InitialState>();
        initial.ShouldHaveUmlBehaviors("""
            TransitionTo(STATE_1)
            """);

        sm.Child("STATE_1").ShouldHaveUmlBehaviors("""
            EV1 TransitionTo(STATE_2)
        """);

        sm.Child("STATE_2").ShouldHaveUmlBehaviors("""
            EV2 TransitionTo(STATE_1)
        """);
    }

    [Fact]
    public void IntegrationTest()
    {
        TestHelper.CaptureSmRunnerFiles(diagramPath: "FlatSmRoot.drawio");
    }
}
