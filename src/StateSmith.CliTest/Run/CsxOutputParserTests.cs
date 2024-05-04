using FluentAssertions;
using StateSmith.Cli.Run;
using System.Collections.Generic;
using Xunit;

namespace StateSmith.CliTest.Run;

public class CsxOutputParserTests
{
    [Fact]
    public void TestDiagramPaths()
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

    [Fact]
    public void TestWrittenFiles()
    {
        CsxOutputParser csxOutputParser = new();
        csxOutputParser.GetPrintedWrittenFilePaths("StateSmith Runner - Writing to file `LightSm.js`")
            .Should().BeEquivalentTo(new List<string> { "LightSm.js" });

        csxOutputParser.GetPrintedWrittenFilePaths("""
            // blah blah
            StateSmith Runner - Writing to file: `LightSm.cpp`
            StateSmith Runner - Writing to file `../LightSm.hh`
            """)
            .Should().BeEquivalentTo(new List<string> { "LightSm.cpp", "../LightSm.hh" });
    }

    [Fact]
    public void TestWrittenFiles2()
    {
        CsxOutputParser csxOutputParser = new();

        csxOutputParser.GetPrintedWrittenFilePaths("""
            StateSmith Runner - Compiling file: `yes1.plantuml` (no state machine name specified).
            StateSmith Runner - State machine `yes` selected.
            StateSmith Runner - Writing to file `yes1.h`
            StateSmith Runner - Writing to file `yes1.c`
            StateSmith Runner - Finished normally.


            StateSmith Runner - Compiling file: `yes2.plantuml` (no state machine name specified).
            StateSmith Runner - State machine `yes` selected.
            StateSmith Runner - Writing to file `yes2.h`
            StateSmith Runner - Writing to file `yes2.c`
            StateSmith Runner - Finished normally.
            """)
            .Should().BeEquivalentTo(new List<string> { "yes1.h", "yes1.c", "yes2.h", "yes2.c" });
    }
}
