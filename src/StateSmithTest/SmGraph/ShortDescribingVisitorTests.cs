using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using System.Text;
using Xunit;

namespace StateSmithTest.SmGraph;

public class ShortDescribingVisitorTests
{
    [Fact]
    public void StandaloneTest()
    {
        TestExpectation(new StateMachine("MySweetSm"), "ROOT");
        TestExpectation(new State("POWERED_ON"), "POWERED_ON");
        TestExpectation(new OrthoState("SUSHI_INSPECTION"), "<Ortho>(SUSHI_INSPECTION)");
        TestExpectation(new RenderConfigVertex(), "<RenderConfig>");
        TestExpectation(new ConfigOptionVertex("CFileIncludes", "blah blah"), "<Config>(CFileIncludes)");
        TestExpectation(new NotesVertex(), "<Notes>");
    }

    [Fact]
    public void RelativeTests()
    {
        State parentState = new("ROBOT_FACE_PLANT");

        TestExpectation(parentState.AddChild(new InitialState()), "ROBOT_FACE_PLANT.<InitialState>");
        TestExpectation(parentState.AddChild(new EntryPoint("recover")), "ROBOT_FACE_PLANT.<EntryPoint>(recover)");
        TestExpectation(parentState.AddChild(new ExitPoint("12")), "ROBOT_FACE_PLANT.<ExitPoint>(12)");
        TestExpectation(parentState.AddChild(new ChoicePoint("55")), "ROBOT_FACE_PLANT.<ChoicePoint>(55)");
        TestExpectation(parentState.AddChild(new HistoryVertex()), "ROBOT_FACE_PLANT.<History>");
        TestExpectation(parentState.AddChild(new HistoryContinueVertex()), "ROBOT_FACE_PLANT.<HistoryContinue>");
    }

    [Fact]
    public void RelativeTestsSkipParent()
    {
        State parentState = new("ROBOT_FACE_PLANT");

        var sb = new StringBuilder();
        var sdv = new ShortDescribingVisitor(sb, skipParentForRelative:true);

        TestExpectation(sdv, parentState.AddChild(new InitialState()), "<InitialState>");
        TestExpectation(sdv, parentState.AddChild(new EntryPoint("recover")), "<EntryPoint>(recover)");
        TestExpectation(sdv, parentState.AddChild(new ExitPoint("12")), "<ExitPoint>(12)");
        TestExpectation(sdv, parentState.AddChild(new ChoicePoint("55")), "<ChoicePoint>(55)");
        TestExpectation(sdv, parentState.AddChild(new HistoryVertex()), "<History>");
        TestExpectation(sdv, parentState.AddChild(new HistoryContinueVertex()), "<HistoryContinue>");
    }

    private static void TestExpectation(Vertex vertex, string Expected)
    {
        ShortDescribingVisitor.Describe(vertex).Should().Be(Expected);
    }

    private static void TestExpectation(ShortDescribingVisitor sdv, Vertex vertex, string Expected)
    {
        sdv.AppendDescription(vertex);
        var str = sdv.PopString();
        str.Should().Be(Expected);
    }
}
