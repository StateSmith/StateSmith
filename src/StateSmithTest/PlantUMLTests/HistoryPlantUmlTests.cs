using System.Collections.Generic;
using Xunit;
using StateSmith.SmGraph;
using FluentAssertions;
using StateSmith.Runner;

namespace StateSmithTest.PlantUMLTests;

public class HistoryPlantUmlTests
{
    private InputSmBuilder inputSmBuilder = new();

    [Fact]
    public void HistoryPlantumlParse_Implicit()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> [H]
            [H] --> S1
            S1-->S2
            @enduml
            """;
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.GetStateMachine();
        InitialState initial = root.ChildType<InitialState>();
        HistoryVertex history = root.ChildType<HistoryVertex>();
        State s1 = root.Child<State>("S1");
        State s2 = root.Child<State>("S2");

        var behaviorMatcher = FABehaviorMatcherBuilder.Build(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

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
        var plantUmlText = """

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

            """;
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.GetStateMachine();
        State relaxing = root.Child<State>("Relaxing");
        HistoryVertex history = relaxing.ChildType<HistoryVertex>();
        State snacking = relaxing.Child<State>("Snacking");
        State interrupted = root.Child<State>("Interrupted");

        var behaviorMatcher = FABehaviorMatcherBuilder.Build(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

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
        var plantUmlText = """

            @startuml ExampleSm
            [*] --> Group[H]
            Group[H] --> S1
            S1-->S2
            @enduml

            """;
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.GetStateMachine();
        InitialState initial = root.ChildType<InitialState>();
        State group = root.Child<State>("Group");
        HistoryVertex history = group.ChildType<HistoryVertex>();
        State s1 = root.Child<State>("S1");
        State s2 = root.Child<State>("S2");

        var behaviorMatcher = FABehaviorMatcherBuilder.Build(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

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
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.GetStateMachine();
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
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();

        StateMachine root = inputSmBuilder.GetStateMachine();
        var map = new NamedVertexMap(inputSmBuilder.GetStateMachine());
        State GetState(string stateName) => map.GetState(stateName);

        GetState("G2").ChildType<HistoryContinueVertex>();
        GetState("G3").ChildType<HistoryContinueVertex>();
    }

}

