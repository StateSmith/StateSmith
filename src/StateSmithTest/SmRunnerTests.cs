using FluentAssertions;
using StateSmith.Runner;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace StateSmithTest.SmRunnerTest;

public class SmRunnerTests
{
    [Fact]
    public void TestOkFilePrintBaseDefault()
    {
        string tempPath = Path.GetTempPath();

        SmRunner runner = new(diagramPath: "test-input/drawio/Design1Sm.drawio.svg", outputDirectory: tempPath);
        StringBuilderConsolePrinter fakeConsole = new();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(fakeConsole);
        runner.Run();

        // have to modify output so that test doesn't rely on temp path because that will vary
        string output = fakeConsole.sb.ToString();
        output = new Regex(@"(StateSmith Runner - Writing to file `).*(/[\w+.]+`)").Replace(output, "$1<snip>$2");
        output = RemoveLibVersionInfo(output);

        //output.Should().Be("");
        output.ConvertLineEndingsToN().ShouldBeShowDiff("""
            
            StateSmith lib ver - <snip>
            StateSmith Runner - Compiling file: `test-input/drawio/Design1Sm.drawio.svg` (no state machine name specified).
            StateSmith Runner - State machine `Design1Sm_svg` selected.
            StateSmith Runner - Writing to file `<snip>/Design1Sm_svg.h`
            StateSmith Runner - Writing to file `<snip>/Design1Sm_svg.c`
            StateSmith Runner - Finished normally.


            """);
    }

    private static string RemoveLibVersionInfo(string output)
    {
        // remove version info: StateSmith lib version: 0.12.2-alpha-1+72cb279852751ea363f5990c364c451026847f2e
        output = new Regex(@"(StateSmith lib ver - ).*").Replace(output, "$1<snip>");
        return output;
    }

    [Fact]
    public void TestOkFilePrintBase2()
    {
        string tempPath = Path.GetTempPath();

        SmRunner runner = new(diagramPath: "test-input/drawio/Design1Sm.drawio.svg", outputDirectory: tempPath);
        runner.Settings.filePathPrintBase = tempPath;
        StringBuilderConsolePrinter fakeConsole = new();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(fakeConsole);
        runner.Run();

        // have to modify output so that test doesn't rely on temp path because that will vary
        string output = fakeConsole.sb.ToString();
        output = new Regex(@"(StateSmith Runner - Compiling file: `).*(/[\w+.]+`)").Replace(output, "$1<snip>$2");
        output = RemoveLibVersionInfo(output);

        output.ConvertLineEndingsToN().ShouldBeShowDiff("""

            StateSmith lib ver - <snip>
            StateSmith Runner - Compiling file: `<snip>/Design1Sm.drawio.svg` (no state machine name specified).
            StateSmith Runner - State machine `Design1Sm_svg` selected.
            StateSmith Runner - Writing to file `Design1Sm_svg.h`
            StateSmith Runner - Writing to file `Design1Sm_svg.c`
            StateSmith Runner - Finished normally.


            """);
    }

    [Fact]
    public void TestReadConfigFromTomlFile()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        string fileTopString = "BigPurpleDinosaur";

        File.WriteAllText(
            Path.Combine(tempDir, "lightbulb.puml"),
            """
            @startuml
            [*] --> on
            on --> off
            off --> on
            @enduml
            """
        );

        File.WriteAllText(
            Path.Combine(tempDir, "lightbulb.toml"),
            $"""
            SmRunnerSettings.transpilerId = "Java"
            RenderConfig.Java.Extends = "{fileTopString}"
            """
        );

        SmRunner runner = new(diagramPath: Path.Combine(tempDir, "lightbulb.puml"), outputDirectory: tempDir);
        runner.Run();

        var javaPath = Path.Combine(tempDir, "lightbulb.java");
        Assert.True(File.Exists(javaPath), $"Java file {javaPath} should be created");
        string javaContent = File.ReadAllText(javaPath);
        javaContent.Should().Contain(fileTopString);

        Directory.Delete(tempDir, true);
    }


    // todo_low - add test for when state machine name is specified

    // todo_low - add test for when exception is thrown

}
