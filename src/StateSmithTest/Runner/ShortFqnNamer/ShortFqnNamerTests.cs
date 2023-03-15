using FluentAssertions;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;
using Xunit;

namespace StateSmithTest.Runner.ShortFqnNamer;

public class ShortFqnNamerTests
{
    [Fact]
    public void Test1()
    {
        SmRunner runner = new(diagramPath: "ShortFqn1.drawio");
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, (TransformationStep)HierachicalGraphToSmConverter.Convert);
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_FinalValidation, (TransformationStep)Test);
        runner.Settings.propagateExceptions = true; // for unit testing
        runner.Settings.outputDirectory = Path.GetTempPath(); // for unit testing
        runner.Run();

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("A");
            map.GetState("A__S1");
            map.GetState("A__S2");
            map.GetState("A__G1");
            map.GetState("A__G2");

            map.GetState("B");
            map.GetState("B__S1");
            map.GetState("B__S2");
            map.GetState("B__G1");
            map.GetState("B__G2");
        }
    }

    [Fact]
    public void TestGraphConverter()
    {
        SmRunner runner = new(diagramPath: "HierachicalGraphConverterEx1.drawio");
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, (TransformationStep)HierachicalGraphToSmConverter.Convert);
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_FinalValidation, (TransformationStep)Test);
        runner.Settings.propagateExceptions = true; // for unit testing
        runner.Settings.outputDirectory = Path.GetTempPath(); // for unit testing
        runner.Run();

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("A").Parent.Should().Be(sm);
            map.GetState("A").ShouldHaveChildrenAndUmlBehaviors("S1", "A");
            map.GetState("S1").ShouldHaveChildrenAndUmlBehaviors("S2", "A__S1");
            map.GetState("S2").ShouldHaveChildrenAndUmlBehaviors("G1, G2", "A__S2");
            map.GetState("G1").ShouldHaveChildrenAndUmlBehaviors("", "A__G1");
            map.GetState("G2").ShouldHaveChildrenAndUmlBehaviors("", "A__G2");
        }
    }
}
