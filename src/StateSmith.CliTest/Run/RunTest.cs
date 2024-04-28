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

        RunHandler runHandler = new(manifest:null, ExamplesHelper.GetExamplesDir() + "/1/");
        runHandler.IgnorePath("a/a3");
        runHandler.Run(recursive: true);
    }
}
