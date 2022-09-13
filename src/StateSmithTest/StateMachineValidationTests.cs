using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest.InitialStateProcessor
{
    public class StateMachineValidationTests : ValidationTestHelper
    {
        InitialState initialStateVertex;
        State s1;
        State s2;
        NotesVertex notesVertex;

        public StateMachineValidationTests()
        {
            var sm = BuildTestGraph();
            compiler.rootVertices = new List<Vertex>() { sm };
        }

        private Vertex BuildTestGraph()
        {
            var sm = new Statemachine(name: "root");

            s1 = sm.AddChild(new State(name: "s1"));
            s2 = sm.AddChild(new State(name: "s2"));
            notesVertex = s1.AddChild(new NotesVertex());

            initialStateVertex = sm.AddChild(new InitialState());
            initialStateVertex.AddTransitionTo(s1);

            return sm;
        }

        [Fact]
        public void NoNestingYet()
        {
            s1.AddChild(new Statemachine("sm2"));
            ExpectVertexValidationException(exceptionMessagePart: "State machines cannot be nested, yet");
        }
    }
}
