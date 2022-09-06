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
            const string filepath = "../../../../../examples/Tiny2-with-noted-out-states.graphml";
            compiler.CompileFile(filepath); //should not throw
        }

        [Fact]
        public void EdgeToNotes()
        {
            Compiler compiler = new Compiler();
            const string filepath = "../../../../../examples/bad-edge-to-notes.graphml";

            Action action = () => compiler.CompileFile(filepath);

            action.Should()
                .Throw<DiagramEdgeException>()
                .WithMessage($"*Failed while converting {nameof(DiagramEdge)} to state transition*")
                .WithInnerException<DiagramNodeException>()
                .WithMessage($"*Could not find State for {nameof(DiagramNode)} with id*");
        }

        [Fact]
        public void EdgeFromNotes()
        {
            Compiler compiler = new Compiler();
            const string filepath = "../../../../../examples/bad-edge-from-notes.graphml";

            Action action = () => compiler.CompileFile(filepath);

            action.Should()
                .Throw<DiagramEdgeException>()
                .WithMessage($"*Failed while converting {nameof(DiagramEdge)} to state transition*")
                .WithInnerException<DiagramNodeException>()
                .WithMessage($"*Could not find State for {nameof(DiagramNode)} with id*");
        }
    }
}
