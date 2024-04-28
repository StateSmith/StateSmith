using FluentAssertions;
using StateSmith.Cli.Run;
using System.Collections.Generic;
using Xunit;

namespace StateSmith.CliTest.Run;

public class CsxOutputParserTests
{
    [Fact]
    public void Test()
    {
        CsxOutputParser csxOutputParser = new();
        csxOutputParser.GetPrintedDiagramPaths("StateSmith Runner - Compiling file: `LightSm.drawio.svg`")
            .Should().BeEquivalentTo(new List<string> { "LightSm.drawio.svg" });

        csxOutputParser.GetPrintedDiagramPaths("""
            // blah blah
            StateSmith Runner - Compiling file: `LightSm.drawio.svg`


            StateSmith Runner - Compiling file: `../TrafficSm.plantuml`

            """)
            .Should().BeEquivalentTo(new List<string> { "LightSm.drawio.svg", "../TrafficSm.plantuml" });
    }
}
