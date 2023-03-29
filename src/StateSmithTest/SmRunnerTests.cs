using FluentAssertions;
using StateSmith.Runner;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace StateSmithTest.SmRunnerTest;

public class SmRunnerTests
{
    [Fact]
    public void TestOkFilePrintBaseDefault()
    {
        string tempPath = Path.GetTempPath();

        SmRunner runner = new(diagramPath: "test-input/drawio/Design1Sm.drawio.svg", outputDirectory: tempPath);
        StringBufferConsolePrinter fakeConsole = new();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(fakeConsole);
        runner.Run();

        // have to modify output so that test doesn't rely on temp path because that will vary
        string output = fakeConsole.sb.ToString();
        output = new Regex(@"(StateSmith Runner - Writing to file `).*(/[\w+.]+`)").Replace(output, "$1<snip>$2");

        //output.Should().Be("");
        output.ConvertLineEndingsToN().ShouldBeShowDiff("""

            StateSmith Runner - Compiling file: `test-input/drawio/Design1Sm.drawio.svg` (no state machine name specified).
            StateSmith Runner - State machine `Design1Sm_svg` selected.
            StateSmith Runner - Writing to file `<snip>/Design1Sm_svg.h`
            StateSmith Runner - Writing to file `<snip>/Design1Sm_svg.c`
            StateSmith Runner - Finished normally.


            """);
    }

    [Fact]
    public void TestOkFilePrintBase2()
    {
        string tempPath = Path.GetTempPath();

        SmRunner runner = new(diagramPath: "test-input/drawio/Design1Sm.drawio.svg", outputDirectory: tempPath);
        runner.Settings.filePathPrintBase = tempPath;
        StringBufferConsolePrinter fakeConsole = new();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(fakeConsole);
        runner.Run();

        // have to modify output so that test doesn't rely on temp path because that will vary
        string output = fakeConsole.sb.ToString();
        output = new Regex(@"(StateSmith Runner - Compiling file: `).*(/[\w+.]+`)").Replace(output, "$1<snip>$2");

        output.ConvertLineEndingsToN().ShouldBeShowDiff("""

            StateSmith Runner - Compiling file: `<snip>/Design1Sm.drawio.svg` (no state machine name specified).
            StateSmith Runner - State machine `Design1Sm_svg` selected.
            StateSmith Runner - Writing to file `Design1Sm_svg.h`
            StateSmith Runner - Writing to file `Design1Sm_svg.c`
            StateSmith Runner - Finished normally.


            """);
    }

    // todo_low - add test for when state machine name is specified

    // todo_low - add test for when exception is thrown

}
