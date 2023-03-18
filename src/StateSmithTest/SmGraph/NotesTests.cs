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
}
