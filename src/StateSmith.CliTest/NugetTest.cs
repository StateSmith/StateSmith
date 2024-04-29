using System;
using Xunit;
using StateSmith.Cli.Utils;

namespace StateSmithCliTest;

public class NugetTest
{
    [Fact]
    public void Test1()
    {
        NugetVersionGrabber.RunAsync().Wait();
    }
}
