using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest.InitialStateProcessor
{
    public class NotesValidationTests : ValidationTestHelper
    {
        InitialState initialStateVertex;
        State s1;
        State s2;
        NotesVertex notesVertex;

        public NotesValidationTests()
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
        public void IncomingTransitions()
        {
            s1.AddTransitionTo(notesVertex);
            ExpectValidationException(exceptionMessagePart: "Notes vertices cannot have any incoming transitions");
        }

        [Fact]
        public void NoBehaviors()
        {
            notesVertex.AddBehavior(new Behavior());
            ExpectValidationException(exceptionMessagePart: "Notes vertices cannot have any behaviors");
        }

        [Fact]
        public void NoChildren()
        {
            notesVertex.AddChild(new State("blah"));
            ExpectValidationException(exceptionMessagePart: "Notes vertices cannot have any children");
        }
    }
}
