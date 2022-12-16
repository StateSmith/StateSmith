using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.Input;
using StateSmith.Runner;

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
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "bad-edge-to-notes.graphml";
            Action action = () => CompileYedFile(filepath);

            action.Should()
                .Throw<DiagramEdgeException>()
                .WithMessage($"*Failed while converting {nameof(DiagramEdge)} with id:* to state transition*")
                .WithInnerException<DiagramNodeException>()
                .WithMessage($"*Could not find State for {nameof(DiagramNode)} with id*");
        }

        [Fact]
        public void EdgeFromNotes()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "/bad-edge-from-notes.graphml";
            Action action = () => CompileYedFile(filepath);

            action.Should()
                .Throw<DiagramEdgeException>()
                .WithMessage($"*Failed while converting {nameof(DiagramEdge)} with id:* to state transition*")
                .WithInnerException<DiagramNodeException>()
                .WithMessage($"*Could not find State for {nameof(DiagramNode)} with id*");
        }
    }
}
