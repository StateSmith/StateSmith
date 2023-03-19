#nullable enable

using FluentAssertions;
using StateSmith.Output.UserConfig;
using System.Collections.Generic;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

public class JsAutoVarsParserTest
{
    readonly JsAutoVarsParser autoVarsParser = new();

    private List<string> ParseIdentifiers(string str) => autoVarsParser.ParseIdentifiers(str);

    [Fact]
    public void Parse()
    {
        ParseIdentifiers("""
            /* some_comment : stuff, */
            blah : undefined,
            count:0,
            """).Should().BeEquivalentTo("blah, count".Split(", "));
    }

    [Fact]
    public void ParseNoEndingComma()
    {
        ParseIdentifiers("""
            /* some_comment : stuff, */
            blah : undefined,
            count:0
            """).Should().BeEquivalentTo("blah, count".Split(", "));
    }
}
