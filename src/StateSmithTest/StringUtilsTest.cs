using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Output;

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
}
