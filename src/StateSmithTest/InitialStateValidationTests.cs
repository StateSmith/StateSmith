using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using System.Linq;

namespace StateSmithTest.InitialStateProcessor
{
    public class InitialStateValidationTests : ValidationTestHelper
    {
        InitialState initialStateVertex;
        private Statemachine sm;
        State s1;
        State s2;

        public InitialStateValidationTests()
        {
            var sm = BuildTestGraph();
            compiler.rootVertices = new List<Vertex>() { sm };
        }

        private Vertex BuildTestGraph()
        {
            sm = new Statemachine(name: "root");
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
            ExpectVertexValidationException(exceptionMessagePart: "children");
        }

        [Fact]
        public void Parent()
        {
            initialStateVertex._parent = null;
            ExpectVertexValidationException(exceptionMessagePart: "parent");
        }

        [Fact]
        public void TargetOutsideOfParent()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddTransitionTo(s2);
            ExpectVertexValidationException(exceptionMessagePart: "transition must remain within parent");
        }

        [Fact]
        public void TargetParent()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddTransitionTo(s1);
            ExpectVertexValidationException(exceptionMessagePart: "cannot target parent");
        }

        [Fact]
        public void StateMachineMustHaveExactly1()
        {
            sm.RemoveChild(initialStateVertex);
            ExpectVertexValidationExceptionWildcard("State machines must have exactly 1 initial state. Actual count: 0.");

            sm.AddChild(new InitialState());
            sm.AddChild(new InitialState());
            ExpectVertexValidationExceptionWildcard("State machines must have exactly 1 initial state. Actual count: 2.");
        }

        [Fact]
        public void TooManySiblings()
        {
            s2.AddChild(new InitialState());
            s2.AddChild(new InitialState());
            ExpectVertexValidationExceptionWildcard("*can only have a single initial state*2*");

            s2.AddChild(new InitialState());
            ExpectVertexValidationExceptionWildcard("*can only have a single initial state*3*");
        }

        [Fact]
        public void NoDefaultTransition()
        {
            initialStateVertex.Behaviors.Single().guardCode = "x > 100";
            ExpectVertexValidationExceptionWildcard("*default transition*");
        }

        [Fact]
        public void GuardOkWithDefaultTransition()
        {
            initialStateVertex.Behaviors.Single().guardCode = "x > 100";
            initialStateVertex.AddTransitionTo(s1); // default no-guard transition
            ExpectValid();
        }

        [Fact]
        public void TrueGuardOkAsDefaultTransition()
        {
            initialStateVertex.Behaviors.Single().guardCode = "true";
            ExpectValid();
        }

        [Fact]
        public void MultipleBehavior()
        {
            initialStateVertex.AddTransitionTo(s1);
            ExpectValid();
        }

        [Fact]
        public void MustHaveTransition()
        {
            var badInitialState = s1.AddChild(new InitialState());
            badInitialState.AddBehavior(new Behavior());
            ExpectVertexValidationException(exceptionMessagePart: "must have a transition target");
        }

        [Fact]
        public void Trigger()
        {
            initialStateVertex.Behaviors[0].triggers.Add("do");
            ExpectVertexValidationException(exceptionMessagePart: "trigger");
        }

        [Fact]
        public void IncomingTransitions()
        {
            s1.AddTransitionTo(initialStateVertex);
            ExpectValid();
        }
    }
}
