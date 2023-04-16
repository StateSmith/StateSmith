using FluentAssertions;
using Xunit.Abstractions;
using StateSmith.Input.Antlr4;
using System.Collections.Generic;

namespace StateSmithTest.Antlr;

public class CommonTestHelper
{
    protected ITestOutputHelper output;
    protected LabelParser parser = new();

    protected void AssertNoErrors()
    {
        parser.HasError().Should().BeFalse();
    }

    protected void AssertErrors()
    {
        parser.HasError().Should().BeTrue();
    }

    protected Node ParseNodeWithAtLeastOneError(string input)
    {
        parser = new LabelParser();
        var result = parser.ParseNodeLabel(input);
        AssertErrors();
        return result;
    }

    protected Node ParseNodeWithNoErrors(string input)
    {
        parser = new LabelParser();
        var result = parser.ParseNodeLabel(input);
        AssertNoErrors();
        return result;
    }

    protected List<NodeBehavior> ParseEdgeWithoutErrors(string input)
    {
        var behaviors = parser.ParseEdgeLabel(input);
        AssertNoErrors();
        return behaviors;
    }

}
