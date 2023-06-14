using System.Collections.Generic;
using Xunit;
using StateSmith.SmGraph;

namespace StateSmithTest;

public class StateMachineValidationTests : ValidationTestHelper
{
    private const string expectedInvalidTriggerErrorText = "A transition behavior cannot have an enter or exit trigger.";

    InitialState initialStateVertex;
    State s1;
    State s2;

    public StateMachineValidationTests()
    {
        var sm = BuildTestGraph();
        diagramToSmConverter.rootVertices = new List<Vertex>() { sm };
    }

    private Vertex BuildTestGraph()
    {
        var sm = new StateMachine(name: "SomeSm");

        s1 = sm.AddChild(new State(name: "s1"));
        s2 = sm.AddChild(new State(name: "s2"));
        s1.AddChild(new NotesVertex());

        initialStateVertex = sm.AddChild(new InitialState());
        initialStateVertex.AddTransitionTo(s1);

        return sm;
    }

    [Fact]
    public void NoNestingYet()
    {
        s1.AddChild(new StateMachine("sm2"));
        ExpectVertexValidationException(exceptionMessagePart: "State machines cannot be nested, yet");
    }

    [Fact]
    public void InvalidTransitionTrigger1()
    {
        var transition = s1.AddTransitionTo(s2);
        transition._triggers.Add("enter");
        ExpectBehaviorValidationException(exceptionMessagePart: expectedInvalidTriggerErrorText);
    }

    [Fact]
    public void InvalidTransitionTrigger2()
    {
        var transition = s1.AddTransitionTo(s2);
        transition._triggers.Add("entry");
        ExpectBehaviorValidationException(exceptionMessagePart: expectedInvalidTriggerErrorText);
    }

    [Fact]
    public void InvalidTransitionTrigger3()
    {
        var transition = s1.AddTransitionTo(s2);
        transition._triggers.Add("exit");
        ExpectBehaviorValidationException(exceptionMessagePart: expectedInvalidTriggerErrorText);
    }

    [Fact]
    public void InvalidTransitionTriggerInList()
    {
        var transition = s1.AddTransitionTo(s2);
        transition._triggers.Add("do");
        transition._triggers.Add("exit");
        ExpectBehaviorValidationException(exceptionMessagePart: expectedInvalidTriggerErrorText);
    }

    [Fact]
    public void DuplicateStateNames()
    {
        s1.AddChild(new State("s2"));
        // Note! We only get this failure because this test disables auto name conflict resolution
        ExpectVertexValidationExceptionWildcard(s2, "*name `s2` also used by state `ROOT.s1.s2`*");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/199
    /// </summary>
    [Fact]
    public void UsingReservedRootName()
    {
        var badRoot = s1.AddChild(new State("root"));
        ExpectVertexValidationExceptionWildcard(badRoot, "*`ROOT`*reserved*");
    }
}
