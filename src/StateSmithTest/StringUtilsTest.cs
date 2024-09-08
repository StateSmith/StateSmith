using Xunit;
using FluentAssertions;
using StateSmith.Output;
using System.Text;

namespace StateSmithTest;

public class StringUtilsTest
{
    [Fact]
    public void DeIndentTrim()
    {
        var input = @"
                My Text Line
            ";
        var output = @"My Text Line";
        StringUtils.DeIndentTrim(input).Should().Be(output);
    }

    [Fact]
    public void DeIndentTrim2()
    {
        var input = StringUtils.ConvertToSlashNLines(@"

                My Text Line

            ");
        var output = "\nMy Text Line\n";
        StringUtils.DeIndentTrim(input).Should().Be(output);
    }

    [Fact]
    public void DeIndentTrim3()
    {
        var input = StringUtils.ConvertToSlashNLines(@"
                My Text Line
                    Next line
            ");
        var output = "My Text Line\n    Next line";
        StringUtils.DeIndentTrim(input).Should().Be(output);
    }

    [Fact]
    public void SnakeCaseToCamelCase()
    {
        StringUtils.SnakeCaseToCamelCase("state_id").Should().Be("stateId");
        StringUtils.SnakeCaseToCamelCase("state__id").Should().Be("state_Id");  // not sure if this is desired. Just showing how it works.
    }

    [Fact]
    public void SnakeCaseToPascalCase()
    {
        StringUtils.SnakeCaseToPascalCase("state_id").Should().Be("StateId");
        StringUtils.SnakeCaseToPascalCase("   state__id").Should().Be("State_Id");  // not sure if this is desired. Just showing how it works.
    }

    [Fact]
    public void FindLastIndent()
    {
        StringBuilder sb = new();

        // no input
        sb.Clear();
        sb.Append("");
        StringUtils.FindLastIndent(sb).Should().Be("");

        // only spaces
        sb.Clear();
        sb.Append("     ");
        StringUtils.FindLastIndent(sb).Should().Be("     ");

        // no indent
        sb.Clear();
        sb.Append("//blah blah ");
        StringUtils.FindLastIndent(sb).Should().Be("");

        // indent at start
        sb.Clear();
        sb.Append("\t \t//blah blah ");
        StringUtils.FindLastIndent(sb).Should().Be("\t \t");

        // indent at end
        sb.Clear();
        sb.Append("    //blah blah       \n  \t");
        StringUtils.FindLastIndent(sb).Should().Be("  \t");

        // indent at end
        sb.Clear();
        sb.Append("    //blah blah       \n  \t  ");
        StringUtils.FindLastIndent(sb).Should().Be("  \t  ");

        sb.Clear();
        sb.Append("    //blah blah       \n   blah  \t  ");
        StringUtils.FindLastIndent(sb).Should().Be("   ");
    }
}
