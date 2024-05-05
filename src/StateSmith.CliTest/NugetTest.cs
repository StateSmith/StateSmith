using System;
using Xunit;
using StateSmith.Cli.Utils;
using Spectre.Console;

namespace StateSmithCliTest;

public class NugetTest
{
    [Fact]
    public void Test1()
    {
        NugetVersionGrabber.RunAsync(AnsiConsole.Console).Wait();
    }
}
