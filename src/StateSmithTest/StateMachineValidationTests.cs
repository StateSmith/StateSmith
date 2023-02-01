using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest
{
    public class StateMachineValidationTests : ValidationTestHelper
    {
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
            var sm = new StateMachine(name: "root");

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
        public void DuplicateStateNames()
        {
            s1.AddChild(new State("s2"));
            ExpectVertexValidationExceptionWildcard(s2, "*name `s2` also used by state `Statemachine{root}.State{s1}.State{s2}`*");
        }
    }
}
