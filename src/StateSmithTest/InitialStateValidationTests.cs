using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest.InitialStateProcessor
{
    public class InitialStateValidationTests : ValidationTestHelper
    {
        InitialState initialStateVertex;
        State s1;
        State s2;

        public InitialStateValidationTests()
        {
            var sm = BuildTestGraph();
            compiler.rootVertices = new List<Vertex>() { sm };
        }

        private Vertex BuildTestGraph()
        {
            var sm = new Statemachine(name: "root");

            s1 = sm.AddChild(new State(name: "s1"));
            s2 = sm.AddChild(new State(name: "s2"));

            initialStateVertex = sm.AddChild(new InitialState());
            initialStateVertex.AddTransitionTo(s1);

            return sm;
        }

        [Fact]
        public void Children()
        {
            initialStateVertex.AddChild(new State("s100"));
            ExpectValidationException(exceptionMessagePart: "children");
        }

        [Fact]
        public void Parent()
        {
            initialStateVertex._parent = null;
            ExpectValidationException(exceptionMessagePart: "parent");
        }

        [Fact]
        public void TargetOutsideOfParent()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddTransitionTo(s2);
            ExpectValidationException(exceptionMessagePart: "transition must remain within parent");
        }

        [Fact]
        public void TargetParent()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddTransitionTo(s1);
            ExpectValidationException(exceptionMessagePart: "cannot target parent");
        }

        [Fact]
        public void TargetSelf()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddTransitionTo(badInitialState);
            ExpectValidationException(exceptionMessagePart: "cannot have any incoming transitions");
        }

        [Fact]
        public void TooManySiblings()
        {
            initialStateVertex.Parent.AddChild(new InitialState());
            ExpectValidationExceptionWildcard("*can only have a single initial state*2*");

            initialStateVertex.Parent.AddChild(new InitialState());
            ExpectValidationExceptionWildcard("*can only have a single initial state*3*");
        }

        [Fact]
        public void SingleBehavior()
        {
            initialStateVertex.AddTransitionTo(s1);
            ExpectValidationException(exceptionMessagePart: "exactly one behavior");
        }

        [Fact]
        public void MustHaveTransition()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddBehavior(new Behavior());
            ExpectValidationException(exceptionMessagePart: "must have a transition target");
        }

        [Fact]
        public void GuardCode()
        {
            initialStateVertex.Behaviors[0].guardCode = "some_code()";
            ExpectValidationException(exceptionMessagePart: "guard code");
        }

        [Fact]
        public void Trigger()
        {
            initialStateVertex.Behaviors[0].triggers.Add("do");
            ExpectValidationException(exceptionMessagePart: "trigger");
        }

        [Fact]
        public void IncomingTransitions()
        {
            s1.AddTransitionTo(initialStateVertex);
            ExpectValidationException(exceptionMessagePart: "incoming");
        }
    }
}
