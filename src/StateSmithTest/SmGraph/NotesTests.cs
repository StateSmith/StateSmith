using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.Runner;

namespace StateSmithTest.SmGraph;

public class NotesTests
{
    readonly InputSmBuilder inputSmBuilder = new();

    private void CompileDrawioFile(string relativeFilePath)
    {
        inputSmBuilder.ConvertDrawIoFileNodesToVertices(TestHelper.GetThisDir() + relativeFilePath);
    }

    [Fact]
    public void ValidNotesCommentingOutStates()
    {
        string filepath = "notes-nested-states.drawio";
        CompileDrawioFile(filepath); //should not throw
    }

    [Fact]
    public void EdgeToNotes()
    {
        string filepath = "notes-bad-edge-to-noted.drawio";
        Action action = () => { CompileDrawioFile(filepath); inputSmBuilder.FinishRunning(); };

        action.Should()
            .Throw<BehaviorValidationException>()
            .WithMessage($"Transition from non-notes section to notes section detected*");
    }

    //[Fact]
    //public void EdgeFromNotes()
    //{
    //    string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "notes/bad-edge-from-notes.graphml";
    //    Action action = () => { CompileYedFile(filepath); inputSmBuilder.FinishRunning(); };

    //    action.Should()
    //        .Throw<BehaviorValidationException>()
    //        .WithMessage($"Transition from notes section to non-notes section detected*");
    //}


    [Fact]
    public void ExhaustiveDynamicTest()
    {
        string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "notes/dynamic_test.drawio.svg";
        inputSmBuilder.ConvertDrawIoFileNodesToVertices(filepath);
        inputSmBuilder.FindSingleStateMachine();
        inputSmBuilder.SetupForSingleSm();
        var sm = inputSmBuilder.GetStateMachine();
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
