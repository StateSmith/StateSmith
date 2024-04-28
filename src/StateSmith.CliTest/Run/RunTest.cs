using StateSmith.Cli.Run;
using Xunit;

namespace StateSmith.CliTest.Run;

public class RunTest
{
    [Fact]
    public void Test()
    {
        RunHandler runHandler = new();
        runHandler.Run(ExamplesHelper.GetExamplesDir() + "/1");
    }
}
