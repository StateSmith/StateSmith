using Xunit;
using FluentAssertions;
using StateSmith.Input.PlantUML;
using StateSmith.Input;
using StateSmith.Compiling;
using System.Collections.Generic;
using System;
using System.Linq;

namespace StateSmithTest;

public class OrderAndElseProcessorTests
{
    private PlantUMLToNodesEdges translator = new();
    private Compiler compiler = new();

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void Basic()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        compiler.SupportElseTriggerAndOrderBehaviors();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ActionAllowed()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else / x++;
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        compiler.SupportElseTriggerAndOrderBehaviors();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ReorderingHappens()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            [*] --> State3 : else / c++;
            [*] --> State1 : [x == 1] / a++;
            [*] --> State2 : [x == 2] / b++;
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        compiler.SupportElseTriggerAndOrderBehaviors();

        var initialState = compiler.rootVertices.Single().Children.OfType<InitialState>().Single();
        initialState.Behaviors[0].actionCode.Should().Be("a++;");
        initialState.Behaviors[1].actionCode.Should().Be("b++;");
        initialState.Behaviors[2].actionCode.Should().Be("c++;");
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ReorderingHappensFromState()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            State0 --> State3 : else / c++;
            State0 --> State2 : 10.0. / b++;
            State0 --> State1 : 1. / a++;
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        compiler.SupportElseTriggerAndOrderBehaviors();

        var state = compiler.GetVertex("State0");
        state.Behaviors[0].actionCode.Should().Be("a++;");
        state.Behaviors[1].actionCode.Should().Be("b++;");
        state.Behaviors[2].actionCode.Should().Be("c++;");
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void NoGuardAllowed()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : [x == 2]
            [*] --> State3 : else [x > 10]
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        Action action = () => compiler.SupportElseTriggerAndOrderBehaviors();
        action.Should().Throw<Exception>();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void OnlyOneElseAllowed()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : else
            [*] --> State3 : else
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        Action action = () => compiler.SupportElseTriggerAndOrderBehaviors();
        action.Should().Throw<Exception>();
    }

    // https://github.com/StateSmith/StateSmith/issues/59
    [Fact]
    public void ElseMustBeOnTransition()
    {
        ParseAssertNoError(@"
            @startuml SomeSmName
            [*] --> State1 : [x == 1]
            [*] --> State2 : else
            State2 : [x > 10] / y++;
            State2 : else / x++;
            @enduml
        ");

        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        Action action = () => compiler.SupportElseTriggerAndOrderBehaviors();
        action.Should().Throw<Exception>();
    }

    private void ParseAssertNoError(string input)
    {
        translator.ParseDiagramText(input);
        translator.HasError().Should().BeFalse();
    }
}
