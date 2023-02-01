using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.Input;
using StateSmith.Runner;
using StateSmith.compiler;

namespace StateSmithTest
{
    public class NotesTests
    {
        CompilerRunner compilerRunner = new();

        private void CompileYedFile(string filepath)
        {
            compilerRunner.CompileYedFileNodesToVertices(filepath);
        }

        [Fact]
        public void ValidNotesCommentingOutStates()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "Tiny2-with-noted-out-states.graphml";
            CompileYedFile(filepath); //should not throw
        }

        [Fact]
        public void EdgeToNotes()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "notes/bad-edge-to-notes.graphml";
            Action action = () => { CompileYedFile(filepath); compilerRunner.FinishRunningCompiler(); };

            action.Should()
                .Throw<BehaviorValidationException>()
                .WithMessage($"Transition from non-notes section to notes section detected*");
        }

        [Fact]
        public void EdgeFromNotes()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "notes/bad-edge-from-notes.graphml";
            Action action = () => { CompileYedFile(filepath); compilerRunner.FinishRunningCompiler(); };

            action.Should()
                .Throw<BehaviorValidationException>()
                .WithMessage($"Transition from notes section to non-notes section detected*");
        }

        [Fact]
        public void NoBehaviors()
        {
            var sm = new Statemachine(name: "root");
            State s1 = sm.AddChild(new State(name: "s1"));
            NotesVertex notesVertex = s1.AddChild(new NotesVertex());
            sm.AddChild(new InitialState()).AddTransitionTo(s1);
            notesVertex.AddBehavior(new Behavior());
            
            compilerRunner.SetStateMachineRoot(sm);

            Action action = () => { compilerRunner.FinishRunningCompiler(); };

            action.Should()
                .Throw<VertexValidationException>()
                .WithMessage($"Notes vertices cannot have any behaviors*");
        }

        [Fact]
        public void ExhaustiveDynamicTest()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "notes/dynamic_test.drawio.svg";
            compilerRunner.CompileDrawIoFileNodesToVertices(filepath);
            compilerRunner.FindSingleStateMachine();
            compilerRunner.SetupForSingleSm();
            var sm = compilerRunner.sm;
            var map = new NamedVertexMap(sm);
            State GetState(string stateName) => map.GetState(stateName);

            var g1 = GetState("G1");
            var g1Notes = g1.DescendantsOfType<NotesVertex>();
            var smNotes = sm.Children<NotesVertex>();

            var allVertices = sm.DescendantsOfType<Vertex>();

            List<NotesVertex> topNotes = new(g1Notes);
            topNotes.AddRange(smNotes);

            // specific test
            {
                var notedVertex1 = GetState("ON1_1");
                var notedVertex2 = GetState("ON1_2");
                TestNotedToV(notedVertex1, notedVertex2, expectFailure: true);
                TestVToNoted(notedVertex1, notedVertex2, expectFailure: true);
            }

            foreach (var topNote in topNotes)
            {
                var notesInTopNote = topNote.DescendantsOfType<Vertex>();
                notesInTopNote.Add(topNote);

                foreach (var notedVertex in notesInTopNote)
                {
                    foreach (var v in allVertices)
                    {
                        bool expectFailure = ShouldExpectFailure(topNote, notedVertex, v);
                        TestNotedToV(notedVertex, v, expectFailure);
                        TestVToNoted(notedVertex, v, expectFailure);
                    }
                }
            }


            void ExpectNoError()
            {
                Validate();
            }

            void ExpectError(Vertex a, Vertex b)
            {
                Action action = () =>
                {
                    Validate();
                };

                // written this way so that breakpoints can be set and `a` and `b` inspected.
                try
                {
                    action();
                    Assert.True(false);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("notes", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.True(false);
                    }
                }

                //action.Should().Throw<Exception>().WithMessage($"*notes*", because: $"blah blah {a} {b}");
            }

            void Validate()
            {
                var processor = new NotesProcessor();
                processor.ValidateNotesWithoutRemoving(sm);
            }

            static bool ShouldExpectFailure(NotesVertex topNote, Vertex notedVertex, Vertex v)
            {
                bool cantHaveBehavior = false;
                if (v is NotesVertex)
                {
                    cantHaveBehavior = true;
                }

                if (notedVertex is NotesVertex)
                {
                    cantHaveBehavior = true;
                }

                var expectFailure = cantHaveBehavior || !topNote.ContainsVertex(v);
                return expectFailure;
            }

            void TestNotedToV(Vertex notedVertex, Vertex v, bool expectFailure)
            {
                var b = notedVertex.AddTransitionTo(v);
                b.guardCode = "true";

                if (expectFailure)
                {
                    ExpectError(notedVertex, v);
                }
                else
                {
                    ExpectNoError();
                }

                notedVertex.RemoveBehaviorAndUnlink(b);
            }

            void TestVToNoted(Vertex notedVertex, Vertex v, bool expectFailure)
            {
                var b = v.AddTransitionTo(notedVertex);
                b.guardCode = "true";

                if (expectFailure)
                {
                    ExpectError(notedVertex, v);
                }
                else
                {
                    ExpectNoError();
                }

                v.RemoveBehaviorAndUnlink(b);
            }
        }
    }
}
