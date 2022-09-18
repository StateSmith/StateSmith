using FluentAssertions;
using Xunit.Abstractions;
using StateSmith.Input.antlr4;


namespace StateSmithTest.Antlr;

public class CommonTestHelper
{
    protected ITestOutputHelper output;
    protected LabelParser parser = new LabelParser();

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

}
