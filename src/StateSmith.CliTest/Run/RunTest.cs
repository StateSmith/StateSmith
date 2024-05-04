using Spectre.Console;
using StateSmith.Cli.Run;
using System.Diagnostics;
using Xunit;

namespace StateSmith.CliTest.Run;

public class RunTest
{
    [Fact]
    public void DebuggerTest()
    {
        if (!Debugger.IsAttached)
            return;

        RunHandler runHandler = new(AnsiConsole.Console, ExamplesHelper.GetExamplesDir() + "/1/");
        runHandler.Finder.AddExcludePattern("a/a3");
        runHandler.Finder.SetAsRecursive();
    }
}
