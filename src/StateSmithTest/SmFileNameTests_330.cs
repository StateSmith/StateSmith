using Xunit;
using StateSmithTest.Output;
using System;
using StateSmith.Runner;
using StateSmith.SmGraph;
using FluentAssertions;
using System.Linq;

namespace StateSmithTest;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/330
/// </summary>
public class SmFileNameTests_330
{
    [Fact]
    public void IntegrationTest()
    {
        var plantUmlText = """
            @startuml {fileName}_2
            [*] --> c1
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        var console = new StringBufferConsolePrinter();
        var fileBaseName = "RocketSm" + Guid.NewGuid().ToString().Replace('-', '_');
        var fileName = fileBaseName + ".plantuml";

        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter:fakeFs, consoleCapturer: console, fileName: fileName, transpilerId:TranspilerId.JavaScript);

        var printedConsole = console.sb.ToString();
        Assert.Contains("RocketSm", printedConsole);
        var expectedSmName = fileBaseName + "_2";

        printedConsole.Should().Contain($"State machine `{expectedSmName}` selected.");

        fakeFs.GetSoleCaptureWithName(expectedSmName + ".js");
    }
}
