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

public class HistoryContinueValidationTests : ValidationTestHelper
{
    private Statemachine root;
    private HistoryContinueVertex hc1;
    private HistoryContinueVertex hc2;

    public HistoryContinueValidationTests()
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
                        state "$HC" as hc2
                    }
                }
            }
            @enduml
            """;
        compilerRunner.CompilePlantUmlTextNodesToVertices(plantUmlText);
        compilerRunner.SetupForSingleSm();        
        root = compilerRunner.sm;
        hc1 = root.Descendant("G2").ChildType<HistoryContinueVertex>();
        hc2 = root.Descendant("G3").ChildType<HistoryContinueVertex>();
    }

    [Fact]
    public void Ok()
    {
        RunCompiler();
    }

    [Fact]
    public void Duplicate()
    {
        root.Descendant("G2").AddChild(new HistoryContinueVertex());
        ExpectVertexValidationExceptionWildcard("* 1 HistoryContinue* allowed*. Found *2*");
    }

    [Fact]
    public void NoBehaviorsAllowed()
    {
        hc1.AddBehavior(new Behavior(actionCode: "x++;"));
        ExpectVertexValidationExceptionWildcard("* HistoryContinue* cannot have any behaviors*. Found *1*");
    }

    [Fact]
    public void NoKidsAllowed()
    {
        hc1.AddChild(new State("blahblah"));
        ExpectVertexValidationException(exceptionMessagePart: "children");
    }

    [Fact]
    public void ParentNonNull()
    {
        hc1._parent = null;
        ExpectVertexValidationException(exceptionMessagePart: "parent");
    }

    [Fact]
    public void GapException1()
    {
        hc1.RemoveSelf();
        ExpectVertexValidationException(exceptionMessagePart: "two levels up");
    }

    [Fact]
    public void HistoryInsteadOfHc1Above()
    {
        hc1.Parent.AddChild(new HistoryVertex()).AddTransitionTo(root.Descendant("G3"));
        hc1.RemoveSelf();
        RunCompiler();
    }

    [Fact]
    public void AdditionalHistoryBesideHc1()
    {
        hc1.Parent.AddChild(new HistoryVertex()).AddTransitionTo(root.Descendant("G3"));
        RunCompiler();
    }

}