using Spectre.Console;
using Spectre.Console.Testing;
using StateSmith.Cli.Run;
using System.Diagnostics;
using Xunit;

namespace StateSmith.CliTest.Run;

public class RunTest
{
    readonly string dirOrManifestPath = ExamplesHelper.GetExamplesDir() + "/1/";

    [Fact]
    public void DebuggerTest()
    {
        //if (!Debugger.IsAttached)
        //    return;

        RunHandler runHandler = new(AnsiConsole.Console, dirOrManifestPath, new(), new(currentDirectory: dirOrManifestPath));
        runHandler.Finder.AddExcludePattern("a/a3");
        runHandler.Finder.SetAsRecursive();
    }

    [Fact]
    public void DebuggerTest2()
    {
        if (!Debugger.IsAttached)
            return;

        //var args = "run -hr --no-csx --propagate-exceptions".Split(' ');
        var args = "run -hr --no-csx".Split(' ');

        var program = new Cli.Program(currentDirectory: dirOrManifestPath);
        TestConsole fakeConsole = new();
        program.ParseCommandsAndRun(args, fakeConsole);
    }
}
