using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.Runner;
using System;
using StateSmith.SmGraph.Validation;

namespace StateSmithTest.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/136
/// </summary>
public class ElseGuardProcessorTests
{
    private readonly InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();

    [Fact]
    public void Test1()
    {
        // https://www.plantuml.com/plantuml/duml/SoWkIImgAStDuOhMYbNGrRLJ22v9B4arv89GLWgkOQv-8T6fESMfiL0nX2eZc7HoNecjhI6c0cCK1KQnN0wfUIb0om00
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", """
            @startuml SomeSmName
            [*] --> State1
            State1: EVENT[else]
            State1 --> State2: EVENT[cnt==1]
            State1 --> State3: EVENT[cnt==2]
            @enduml
            """);

        inputSmBuilder.FinishRunning();

        var map = new NamedVertexMap(inputSmBuilder.GetStateMachine());
        map.GetState("State1").ShouldHaveUmlBehaviors("""
            EVENT [cnt==1] TransitionTo(State2)
            EVENT [cnt==2] TransitionTo(State3)
            else EVENT
            """);
    }

    [Fact]
    public void Test2()
    {
        // https://www.plantuml.com/plantuml/duml/SoWkIImgAStDKGZEpqqDplLBp4tbYjQALT3LjLC8BaaiIJNaWb084ICh1TSmLx-GwDISujHO36bZc7HoNecjhL4ibqDgNWhG1W00
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", """
            @startuml SomeSmName
            [*] --> State1
            State1 --> State2: EVENT[else]
            State1 --> State3: EVENT[cnt==2]
            @enduml
            """);

        inputSmBuilder.FinishRunning();

        var map = new NamedVertexMap(inputSmBuilder.GetStateMachine());
        map.GetState("State1").ShouldHaveUmlBehaviors("""
            EVENT [cnt==2] TransitionTo(State3)
            else EVENT TransitionTo(State2)
            """);
    }

    [Fact]
    public void ThrowIfElseAndOrderSpecified()
    {
        // https://www.plantuml.com/plantuml/duml/SoWkIImgAStDuOhMYbNGrRLJ22v9B4arv89GLWg61deAhc6kVY7HgJd5gR5GSOHA8okeT79UYQsj8QO2OnG5Hh5S3gbvAK0B0G00
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", """
            @startuml SomeSmName
            [*] --> State1
            State1: 10. EVENT[else]
            State1 --> State2: EVENT[cnt==1]
            State1 --> State3: EVENT[cnt==2]
            @enduml
            """);

        Action action = () => inputSmBuilder.FinishRunning();
        action.Should().Throw<BehaviorValidationException>().WithMessage("*specify order and `[else]` guard at the same time*");
    }
}
