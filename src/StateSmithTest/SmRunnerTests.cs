using StateSmith.Runner;
using System.IO;
using System.Text;
using Xunit;

namespace StateSmithTest.SmRunnerTest;

public class SmRunnerTests
{
    class FakeConsole : IConsolePrinter
    {
        public readonly StringBuilder sb = new();

        void IConsolePrinter.WriteErrorLine(string message) => sb.AppendLine(message);
        void IConsolePrinter.WriteLine(string message) => sb.AppendLine(message);
        void IConsolePrinter.WriteLine() => sb.AppendLine();
    }

    [Fact]
    public void TestOk()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/Design1Sm.drawio.svg";

        string tempPath = Path.GetTempPath();
        SmRunner runner = new(filePath, outputDirectory: tempPath);

        FakeConsole fakeConsole = new();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(fakeConsole);
        runner.Run();

        fakeConsole.sb.ToString().ConvertLineEndingsToN().ShouldBeShowDiff("""

            StateSmith Runner - Compiling file: `test-input/drawio/Design1Sm.drawio.svg` (no state machine name specified).
            StateSmith Runner - State machine `Design1Sm_svg` selected.
            StateSmith Runner - Finished normally.


            """);
    }

    // todo_low - add test for when state machine name is specified

    // todo_low - add test for when exception is thrown

}
