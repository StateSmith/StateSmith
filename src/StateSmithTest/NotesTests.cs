using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.Input;

namespace StateSmithTest
{
    public class NotesTests
    {
        [Fact]
        public void ValidNotesCommentingOutStates()
        {
            Compiler compiler = new Compiler();
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "Tiny2-with-noted-out-states.graphml";
            compiler.CompileYedFile(filepath); //should not throw
        }

        [Fact]
        public void EdgeToNotes()
        {
            Compiler compiler = new Compiler();
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "bad-edge-to-notes.graphml";

            Action action = () => compiler.CompileYedFile(filepath);

            action.Should()
                .Throw<DiagramEdgeException>()
                .WithMessage($"*Failed while converting {nameof(DiagramEdge)} with id:* to state transition*")
                .WithInnerException<DiagramNodeException>()
                .WithMessage($"*Could not find State for {nameof(DiagramNode)} with id*");
        }

        [Fact]
        public void EdgeFromNotes()
        {
            Compiler compiler = new Compiler();
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "/bad-edge-from-notes.graphml";

            Action action = () => compiler.CompileYedFile(filepath);

            action.Should()
                .Throw<DiagramEdgeException>()
                .WithMessage($"*Failed while converting {nameof(DiagramEdge)} with id:* to state transition*")
                .WithInnerException<DiagramNodeException>()
                .WithMessage($"*Could not find State for {nameof(DiagramNode)} with id*");
        }
    }
}
