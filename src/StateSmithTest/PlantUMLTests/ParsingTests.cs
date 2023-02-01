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
using StateSmith.Runner;
using StateSmithTest;
using StateSmith.compiler;

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
    public void NoDiagramNameThrows()
    {
        Action action = () => ParseAssertNoError(@"
@startuml
State1 --> State2
@enduml
");

        action.Should().Throw<Exception>()
            .WithMessage("PlantUML diagrams need a name and should start like `@startuml MySmName`. Location Details { line: 2, column: 0, text: `@startuml`. }");
    }

    [Fact]
    public void ThrowOnEndState()
    {
        Action action = () => ParseAssertNoError(@"
@startuml SomeName
State1 --> [*]
@enduml
");

        action.Should().Throw<Exception>()
            .WithMessage("StateSmith doesn't support end states. Location Details { line: 3, column: 0, text: `State1 --> [*]`. }");
    }

    [Fact]
    public void TwoStates()
    {
        ParseAssertNoError(@"
@startuml SomeSmName

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

        initialState.label.Should().Be(DiagramToSmConverter.InitialStateString);
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
@startuml DiagramName
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
@startuml SompySm
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

        InputSmBuilder inputSmBuilder = new();
        inputSmBuilder.ConvertNodesToVertices(new List<DiagramNode> { translator.Root }, translator.Edges);
        inputSmBuilder.FinishRunningCompiler();
    }


    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/40
    /// </summary>
    [Fact]
    public void ChoicePoints()
    {
        var plantUmlText = @"
@startuml ExampleSm
state c1 <<choice>>
[*] --> c1
c1 --> s1 : [id <= 10]
c1 --> s2 : else
@enduml
";
        InputSmBuilder inputSmBuilder = new();
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.FinishRunningCompiler();

        StateMachine root = inputSmBuilder.Sm;
        InitialState initial = root.ChildType<InitialState>();
        ChoicePoint c1 = root.ChildType<ChoicePoint>();
        State s1 = root.Child<State>("s1");
        State s2 = root.Child<State>("s2");

        c1.label.Should().Be("c1");

        var behaviorMatcher = VertexTestHelper.BuildFluentAssertionBehaviorMatcher(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

        initial.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = c1 },
        }, behaviorMatcher);

        c1.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = s1, guardCode = "id <= 10" },
            new Behavior(){ _transitionTarget = s2 },
        }, behaviorMatcher);
    }

    [Fact]
    public void UnescapeNewlines()
    {
        // input modified from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError(@"
@startuml SomeSmName
s1 :  / { initial_tx_action(); \n x++; }
[*]-->s1:[\n guard1\n && guard2 ]
@enduml

");

        translator.Edges[0].label.Should().Be("[\n guard1\n && guard2 ]");
        translator.Root.children[0].label.Should().Be("s1\n/ { initial_tx_action(); \n x++; }");
    }

    [Fact]
    public void LotsOfNotesAndComments()
    {
        ParseAssertNoError(@"
@startuml ButtonSm1Cpp

state BetweenNotes1

note ""This is a PlantUML diagram"" as N1

state BetweenNotes2

note left of Active : this is a short\nnote

state BetweenNotes3

note right of Inactive
  A note can also
  state DontFindMe1
state DontFindMe2
end note

'state DontFindMe2

    state BetweenNotes4

        note right of Inactive
          A note can also
          state DontFindMe1
            state DontFindMe2
        end note

state BetweenNotes5

  note right of Inactive
    A note can also
    state DontFindMe1
state DontFindMe2
  endnote

state BetweenNotes6

/'
Shouldn't find this
state DontFindMe1
state DontFindMe2

'/

    /'
    Shouldn't find this
    state DontFindMe1
    state DontFindMe2
    '/


state BetweenNotes7

@enduml
");

        for (int i = 1; i <= 7; i++)
        {
            GetVertexById("BetweenNotes" + i);
        }

        Action a;
        a = () => GetVertexById("DontFindMe1");
        a.Should().Throw<Exception>();
        a = () => GetVertexById("DontFindMe2");
        a.Should().Throw<Exception>();
    }

    [Fact]
    public void SkinparamBlock()
    {
        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state {
            }

            @enduml
            ");

        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state {

            }
            @enduml
            ");

        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state {
            }
            @enduml
            ");

        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state
            {
            }
            @enduml
            ");

        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state
        
            {
            }
            @enduml
            ");

        ParseAssertNoError(@"
@startuml blinky1_printf_sm
skinparam state {
 BorderColor<<on_style>> #AA0000
 BackgroundColor<<on_style>> #ffcccc
 FontColor<<on_style>> darkred
 
 BorderColor Black
}

@enduml
");
        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state {

                BorderColor<<on_style>> #AA0000
                BackgroundColor<<on_style>> #ffcccc
                FontColor<<on_style>> darkred
 
                BorderColor Black

            }
            @enduml
        ");

        ParseAssertNoError(@"
            @startuml blinky1_printf_sm
            skinparam state {
                BackgroundColor<<on_style>> #ffcccc
            '}
            }

            @enduml
        ");
    }

    [Fact]
    public void SkinparamBlockBad()
    {
        ParseAssertHasAtLeastOneError(@"
            @startuml blinky1_printf_sm
            skinparam state {
                BackgroundColor<<on_style>> #ffcccc
                [*] --> B
            }
            @enduml
            ");
    }

    [Fact]
    public void SkinparamBlockBad2()
    {
        ParseAssertHasAtLeastOneError(@"
            @startuml blinky1_printf_sm
            skinparam state {
                BackgroundColor<<on_style>> #ffcccc
            }'
            LED_ON : enter / { action(); }
            @enduml
            ");
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

public class HistoryPlantUmlTests
{
    private InputSmBuilder inputSmBuilder = new();

    [Fact]
    public void HistoryPlantumlParse_Implicit()
    {
        var plantUmlText = @"
@startuml ExampleSm
[*] --> [H]
[H] --> S1
S1-->S2
@enduml
";
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.Sm;
        InitialState initial = root.ChildType<InitialState>();
        HistoryVertex history = root.ChildType<HistoryVertex>();
        State s1 = root.Child<State>("S1");
        State s2 = root.Child<State>("S2");

        var behaviorMatcher = VertexTestHelper.BuildFluentAssertionBehaviorMatcher(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

        initial.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = history },
        }, behaviorMatcher);

        history.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = s1 },
        }, behaviorMatcher);
    }

    [Fact]
    public void HistoryPlantumlParse_ImplicitInGroup()
    {
        var plantUmlText = @"
@startuml ExampleSm
[*] --> Relaxing
state Relaxing {
    [*] --> Reading
    Reading --> Snacking
    Snacking --> Napping
    Napping --> Napping
}
Relaxing --> Interrupted
Interrupted --> Relaxing[H]
Relaxing[H] --> Snacking
@enduml
";
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.Sm;
        State relaxing = root.Child<State>("Relaxing");
        HistoryVertex history = relaxing.ChildType<HistoryVertex>();
        State snacking = relaxing.Child<State>("Snacking");
        State interrupted = root.Child<State>("Interrupted");

        var behaviorMatcher = VertexTestHelper.BuildFluentAssertionBehaviorMatcher(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

        interrupted.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = history },
        }, behaviorMatcher);

        history.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = snacking },
        }, behaviorMatcher);
    }

    [Fact]
    public void HistoryPlantumlParse_ExplicitState()
    {
        var plantUmlText = @"
@startuml ExampleSm
[*] --> Group[H]
Group[H] --> S1
S1-->S2
@enduml
";
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.Sm;
        InitialState initial = root.ChildType<InitialState>();
        State group = root.Child<State>("Group");
        HistoryVertex history = group.ChildType<HistoryVertex>();
        State s1 = root.Child<State>("S1");
        State s2 = root.Child<State>("S2");

        var behaviorMatcher = VertexTestHelper.BuildFluentAssertionBehaviorMatcher(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

        initial.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = history },
        }, behaviorMatcher);

        history.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = s1 },
        }, behaviorMatcher);
    }

    [Fact]
    public void HistoryPlantumlParse_Continue()
    {
        var plantUmlText = """

            @startuml ExampleSm
            hide empty description

            skinparam state {
              BackgroundColor<<hc>> orange
            }

            [*] --> Relaxing
            state Relaxing {
                [*] --> [H]
                [H] --> Reading
                Reading --> Snacking
                Snacking --> Napping

                state Snacking {
                    state "$hc" as hc1<<hc>>
                    [*] --> Toast
                    state Toast
                    state Oatmeal
                }
            }
            Relaxing --> Interrupted
            Interrupted --> Relaxing[H]
            @enduml

            """;
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.Sm;
        var map = new NamedVertexMap(root);
        map.GetState("Snacking").ChildType<HistoryContinueVertex>();
    }

    [Fact]
    public void HistoryPlantumlParse_Continue2()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> G1
            state G1 {
                [*] --> [H]
                [H] --> G2
                state G2 {
                    [*] --> G3
                    state "$HC" as hc1
                    state G3 {
                        [*] --> G4
                        state "$HC" as hc2
                    }
                }
            }
            @enduml
            """;
        inputSmBuilder.CompilePlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.Sm;
        var map = new NamedVertexMap(inputSmBuilder.Sm);
        State GetState(string stateName) => map.GetState(stateName);

        GetState("G2").ChildType<HistoryContinueVertex>();
        GetState("G3").ChildType<HistoryContinueVertex>();
    }

}

