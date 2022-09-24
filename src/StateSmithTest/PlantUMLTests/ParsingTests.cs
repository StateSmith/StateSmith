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

        action.Should().Throw<InvalidOperationException>()
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
        translator.ParseDiagramText(input);
        translator.HasError().Should().BeFalse();
    }

    private void ParseAssertHasAtLeastOneError(string input)
    {
        translator.ParseDiagramText(input);
        translator.HasError().Should().BeTrue();
    }


    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/3
    /// </summary>
    [Fact]
    public void EntryExitStates()
    {
        // input modified from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError(@"
@startuml
state Somp {
  state entry1 <<entryPoint>>
  state entry2 <<entryPoint>>
  state sin
  entry1 --> sin
  entry2 -> sin
  sin -> sin2
  sin2 --> exitA <<exitPoint>>
}

[*] --> entry1 : / initial_tx_action();
exitA --> Foo
Foo1 -> entry2 : EV1 [guard()] / action_e2();
@enduml

");

        DiagramNode root = translator.Root;

        DiagramNode initial = root.children[1];
        DiagramNode Foo = GetVertexById("Foo");
        DiagramNode Foo1 = GetVertexById("Foo1");

        DiagramNode Somp = GetVertexById("Somp");
        DiagramNode entry1 = GetVertexById("entry1");
        DiagramNode entry2 = GetVertexById("entry2");
        DiagramNode exitA = GetVertexById("exitA");
        DiagramNode sin = GetVertexById("sin");
        DiagramNode sin2 = GetVertexById("sin2");

        entry1.label.Should().Be("entry : entry1");
        entry2.label.Should().Be("entry : entry2");
        exitA.label.Should().Be("exit : exitA");

        int i = 0;
        AssertEdge(translator.Edges[i++], source: entry1, target: sin, label: "");
        AssertEdge(translator.Edges[i++], source: entry2, target: sin, label: "");
        AssertEdge(translator.Edges[i++], source: sin, target: sin2, label: "");
        AssertEdge(translator.Edges[i++], source: sin2, target: exitA, label: "");
        // following edges need re-routing and label adjustments
        AssertEdge(translator.Edges[i++], source: initial, target: /* was entry1 */ Somp, label: "/ initial_tx_action();via entry entry1");
        AssertEdge(translator.Edges[i++], source: /* was exitA */ Somp, target: Foo, label: "via exit exitA");
        AssertEdge(translator.Edges[i++], source: Foo1, target: /* was entry2 */ Somp, label: "EV1 [guard()] / action_e2();via entry entry2");

        // ensure entry and exit validation works

        Compiler compiler = new();
        compiler.CompileDiagramNodesEdges(new List<DiagramNode> { translator.Root }, translator.Edges);
        compiler.SetupRoots();
        compiler.SupportParentAlias();
        compiler.Validate();

        compiler.SimplifyInitialStates();
        compiler.SupportEntryExitPoints();
        compiler.Validate();
    }

    [Fact]
    public void UnescapeNewlines()
    {
        // input modified from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError(@"
@startuml
s1 :  / { initial_tx_action(); \n x++; }
[*]-->s1:[\n guard1\n && guard2 ]
@enduml

");

        translator.Edges[0].label.Should().Be("[\n guard1\n && guard2 ]");
        translator.Root.children[0].label.Should().Be("s1\n/ { initial_tx_action(); \n x++; }");
    }

    private DiagramNode GetVertexById(string id)
    {
        return translator.GetDiagramNode(id);
    }

    private static void AssertEdge(DiagramEdge diagramEdge, DiagramNode source, DiagramNode target)
    {
        diagramEdge.source.Should().Be(source);
        diagramEdge.target.Should().Be(target);
    }

    private static void AssertEdge(DiagramEdge diagramEdge, DiagramNode source, DiagramNode target, string label)
    {
        AssertEdge(diagramEdge, source, target);
        diagramEdge.label.Should().Be(label);
    }
}
