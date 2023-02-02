using Xunit;
using FluentAssertions;
using StateSmith.Input.PlantUML;
using StateSmith.SmGraph;
using System;
using System.Linq;
using StateSmith.Runner;

namespace StateSmithTest;

public class OrderAndElseProcessorTests
{
    private PlantUMLToNodesEdges translator = new();
    private InputSmBuilder inputSmBuilder = new();

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void Basic()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else
            @enduml
        ");

        OrderAndElseProcessor.Process(inputSmBuilder.Sm);
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ActionAllowed()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else / x++;
            @enduml
        ");

        OrderAndElseProcessor.Process(inputSmBuilder.Sm);
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ReorderingHappens()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State3 : else / c++;
            [*] --> State1 : [x == 1] / a++;
            [*] --> State2 : [x == 2] / b++;
            @enduml
        ");

        OrderAndElseProcessor.Process(inputSmBuilder.Sm);

        var initialState = inputSmBuilder.Sm.Children.OfType<InitialState>().Single();
        initialState.Behaviors[0].actionCode.Should().Be("a++;");
        initialState.Behaviors[1].actionCode.Should().Be("b++;");
        initialState.Behaviors[2].actionCode.Should().Be("c++;");
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ReorderingHappensFromState()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            State0 --> State3 : else / c++;
            State0 --> State2 : 10.0. / b++;
            State0 --> State1 : 1. / a++;
            @enduml
        ");

        OrderAndElseProcessor.Process(inputSmBuilder.Sm);

        var map = new NamedVertexMap(inputSmBuilder.Sm);
        State GetState(string stateName) => map.GetState(stateName);

        var state = GetState("State0");
        state.Behaviors[0].actionCode.Should().Be("a++;");
        state.Behaviors[1].actionCode.Should().Be("b++;");
        state.Behaviors[2].actionCode.Should().Be("c++;");
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void NoGuardAllowed()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else [x > 10]
            @enduml
        ");

        Action action = () => OrderAndElseProcessor.Process(inputSmBuilder.Sm);
        action.Should().Throw<Exception>();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void OnlyOneElseAllowed()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : else
            [*] --> State3 : else
            @enduml
        ");

        Action action = () => OrderAndElseProcessor.Process(inputSmBuilder.Sm);
        action.Should().Throw<Exception>();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ElseMustBeOnTransition()
    {
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : else
            State2 : [x > 10] / y++;
            State2 : else / x++;
            @enduml
        ");

        Action action = () => OrderAndElseProcessor.Process(inputSmBuilder.Sm);
        action.Should().Throw<Exception>();
    }
}
