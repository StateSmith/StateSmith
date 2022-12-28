using FluentAssertions;
using StateSmith.compiler;
using StateSmith.Compiling;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StateSmithTest.PseudoStateTests;

public class HistoryValidationTests : PseudoStateValidationTestHelper
{
    public HistoryValidationTests()
    {
        AddBlankS2PseudoStateTransition();
    }

    protected override void AddBlankS2PseudoStateTransition()
    {
        s2_pseudoState.AddTransitionTo(s2_1);
    }

    override protected PseudoStateVertex CreateS2PseudoState()
    {
        return new HistoryVertex();
    }

    [Fact]
    public override void MultipleBehavior()
    {
        AddBlankS2PseudoStateTransition();
        ExpectVertexValidationExceptionWildcard("* a single default transition. Found *2*");

        AddBlankS2PseudoStateTransition();
        ExpectVertexValidationExceptionWildcard("* a single default transition. Found *3*");
    }

    [Fact]
    public override void GuardWithDefaultTransition()
    {
        s2_pseudoState.Behaviors.Single().guardCode = "x > 100";
        AddBlankS2PseudoStateTransition(); // default no-guard transition
        ExpectVertexValidationExceptionWildcard("* a single default transition. Found *2*");
    }

    [Fact]
    public void TooManySiblings()
    {
        s2.AddChild(new HistoryVertex());
        ExpectVertexValidationExceptionWildcard("* 1 history* allowed*. Found *2*");
        s2.AddChild(new HistoryVertex());
        ExpectVertexValidationExceptionWildcard("* 1 history* allowed*. Found *3*");
    }
}

public class HistoryPlantUmlTests
{
    private CompilerRunner compilerRunner = new();

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
        compilerRunner.CompilePlantUmlTextNodesToVertices(plantUmlText);
        compilerRunner.SetupForSingleSm();

        Statemachine root = compilerRunner.sm;
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
        compilerRunner.CompilePlantUmlTextNodesToVertices(plantUmlText);
        compilerRunner.SetupForSingleSm();

        Statemachine root = compilerRunner.sm;
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
        compilerRunner.CompilePlantUmlTextNodesToVertices(plantUmlText);
        compilerRunner.SetupForSingleSm();

        Statemachine root = compilerRunner.sm;
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
}

