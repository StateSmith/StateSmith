using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using StateSmith.Input.PlantUML;
using StateSmith.Compiling;
using StateSmith.Input;
using FluentAssertions;

namespace StateSmithTest.PlantUMLTests;


public class ParsingTests
{
    private PlantUMLToNodesEdges translator = new();

    public ParsingTests()
    {

    }


    [Fact]
    public void DiagramName()
    {
        ParseAssertNoError(@"
@startuml MyPumlSm1

@enduml
");
        translator.Root.id.Should().Be("MyPumlSm1");
    }

    [Fact]
    public void InvalidInput()
    {
        ParseAssertHasAtLeastOneError(@"
@startuml MyPumlSm1 Blah

@enduml
");
    }

    [Fact]
    public void DiagramNameDefaultToRoot()
    {
        ParseAssertNoError(@"
@startuml
@enduml
");
        translator.Root.id.Should().Be("ROOT");
    }

    [Fact]
    public void ThrowOnEndState()
    {
        Action action = () => ParseAssertNoError(@"
@startuml
State1 --> [*]
@enduml
");

        action.Should().Throw<ArgumentException>()
            .WithMessage("StateSmith doesn't support end states. Location Details { line: 3, column: 0, text: `State1 --> [*]`. }");
    }

    [Fact]
    public void TwoStates()
    {
        ParseAssertNoError(@"
@startuml

[*] --> State1
State1 : enter / some_action();
State1 : EVENT [guard] { action(); cout << Obj::cpp_rules(); x->v = 100 >> 2; }
State1 -> State2 : EVENT2 [guard2] / tx_action();

@enduml
");
        translator.Root.children.Count.Should().Be(3);

        DiagramNode initialState = translator.Root.children[0];
        DiagramNode state1 = translator.Root.children[1];
        DiagramNode state2 = translator.Root.children[2];

        initialState.label.Should().Be(Compiler.InitialStateString);
        state1.label.Should().Be("State1\nenter / some_action();\nEVENT [guard] { action(); cout << Obj::cpp_rules(); x->v = 100 >> 2; }");
        state2.label.Should().Be("State2");

        translator.Edges.Count.Should().Be(2);
        translator.Edges[0].source.Should().Be(initialState);
        translator.Edges[0].target.Should().Be(state1);
        //
        translator.Edges[1].source.Should().Be(state1);
        translator.Edges[1].target.Should().Be(state2);
        translator.Edges[1].label.Should().Be("EVENT2 [guard2] / tx_action();");
    }

    private void AssertNodeHasNoKids(DiagramNode node)
    {
        node.children.Should().BeEquivalentTo(new List<DiagramNode> {  });
    }

    [Fact]
    public void CompositeStatesWithLongName()
    {
        // mod from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError(@"
@startuml
state A {
  state X {
  }
  state Y {
    state ""Y_1"" as State1 {
        State1 : exit / some_exit_action(); x++; y--;
    }
  }
}
 
state B {
  state Z {
  }
}

X --> Z
Z --> Y
@enduml
");
        DiagramNode root = translator.Root;
        DiagramNode stateA = root.children[0];
        DiagramNode stateB = root.children[1];

        DiagramNode stateX = stateA.children[0];
        DiagramNode stateY = stateA.children[1];

        DiagramNode state1 = stateY.children[0];

        DiagramNode stateZ = stateB.children[0];

        root.children.Should().BeEquivalentTo(new List<DiagramNode> { stateA, stateB });
        stateA.children.Should().BeEquivalentTo(new List<DiagramNode>{ stateX, stateY });
        stateB.children.Should().BeEquivalentTo(new List<DiagramNode>{ stateZ });
        stateY.children.Should().BeEquivalentTo(new List<DiagramNode>{ state1 });
        AssertNodeHasNoKids(stateX);
        AssertNodeHasNoKids(stateZ);

        state1.label.Should().Be("Y_1\nexit / some_exit_action(); x++; y--;");

        translator.Edges.Count.Should().Be(2);
        translator.Edges[0].source.Should().Be(stateX);
        translator.Edges[0].target.Should().Be(stateZ);
        //
        translator.Edges[1].source.Should().Be(stateZ);
        translator.Edges[1].target.Should().Be(stateY);
    }

    private void ParseAssertNoError(string input)
    {
        translator.ParseDiagram(input);
        translator.HasError().Should().BeFalse();
    }

    private void ParseAssertHasAtLeastOneError(string input)
    {
        translator.ParseDiagram(input);
        translator.HasError().Should().BeTrue();
    }
}
