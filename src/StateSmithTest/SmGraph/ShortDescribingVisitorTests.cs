using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using Xunit;

namespace StateSmithTest.SmGraph;

public class ShortDescribingVisitorTests
{
    [Fact]
    public void StandaloneTest()
    {
        TestExpectation(new StateMachine("MySweetSm"), "ROOT");
        TestExpectation(new State("POWERED_ON"), "POWERED_ON");
        TestExpectation(new OrthoState("SUSHI_INSPECTION"), "$ORTHO(SUSHI_INSPECTION)");
        TestExpectation(new RenderConfigVertex(), "$RenderConfig");
        TestExpectation(new ConfigOptionVertex("CFileIncludes", "blah blah"), "$CONFIG(CFileIncludes)");
        TestExpectation(new NotesVertex(), "$NOTES");
    }

    [Fact]
    public void RelativeTests()
    {
        State parentState = new("ROBOT_FACE_PLANT");

        TestExpectation(parentState.AddChild(new InitialState()), "ROBOT_FACE_PLANT.InitialState");
        TestExpectation(parentState.AddChild(new EntryPoint("recover")), "ROBOT_FACE_PLANT.EntryPoint(recover)");
        TestExpectation(parentState.AddChild(new ExitPoint("12")), "ROBOT_FACE_PLANT.ExitPoint(12)");
        TestExpectation(parentState.AddChild(new ChoicePoint("55")), "ROBOT_FACE_PLANT.ChoicePoint(55)");
        TestExpectation(parentState.AddChild(new HistoryVertex()), "ROBOT_FACE_PLANT.History");
        TestExpectation(parentState.AddChild(new HistoryContinueVertex()), "ROBOT_FACE_PLANT.HistoryContinue");
    }

    private static void TestExpectation(Vertex vertex, string Expected)
    {
        ShortDescribingVisitor.Describe(vertex).Should().Be(Expected);
    }
}
