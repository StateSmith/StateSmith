using Xunit;
using FluentAssertions;
using StateSmith.Input.PlantUML;
using StateSmith.Input;
using StateSmith.Compiling;
using System.Collections.Generic;
using System;
using System.Linq;
using StateSmith.Runner;
using StateSmith.compiler;

namespace StateSmithTest;

public class OrderAndElseProcessorTests
{
    private PlantUMLToNodesEdges translator = new();
    private CompilerRunner compilerRunner = new();

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void Basic()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else
            @enduml
        ");

        OrderAndElseProcessor.Process(compilerRunner.sm);
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ActionAllowed()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else / x++;
            @enduml
        ");

        OrderAndElseProcessor.Process(compilerRunner.sm);
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ReorderingHappens()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State3 : else / c++;
            [*] --> State1 : [x == 1] / a++;
            [*] --> State2 : [x == 2] / b++;
            @enduml
        ");

        OrderAndElseProcessor.Process(compilerRunner.sm);

        var initialState = compilerRunner.sm.Children.OfType<InitialState>().Single();
        initialState.Behaviors[0].actionCode.Should().Be("a++;");
        initialState.Behaviors[1].actionCode.Should().Be("b++;");
        initialState.Behaviors[2].actionCode.Should().Be("c++;");
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ReorderingHappensFromState()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            State0 --> State3 : else / c++;
            State0 --> State2 : 10.0. / b++;
            State0 --> State1 : 1. / a++;
            @enduml
        ");

        OrderAndElseProcessor.Process(compilerRunner.sm);

        var state = compilerRunner.sm.Descendant("State0");
        state.Behaviors[0].actionCode.Should().Be("a++;");
        state.Behaviors[1].actionCode.Should().Be("b++;");
        state.Behaviors[2].actionCode.Should().Be("c++;");
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void NoGuardAllowed()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else [x > 10]
            @enduml
        ");

        Action action = () => OrderAndElseProcessor.Process(compilerRunner.sm);
        action.Should().Throw<Exception>();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void OnlyOneElseAllowed()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : else
            [*] --> State3 : else
            @enduml
        ");

        Action action = () => OrderAndElseProcessor.Process(compilerRunner.sm);
        action.Should().Throw<Exception>();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ElseMustBeOnTransition()
    {
        compilerRunner.CompilePlantUmlTextNodesToVertices(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : else
            State2 : [x > 10] / y++;
            State2 : else / x++;
            @enduml
        ");

        Action action = () => OrderAndElseProcessor.Process(compilerRunner.sm);
        action.Should().Throw<Exception>();
    }
}
