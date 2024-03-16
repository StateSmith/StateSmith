using System;
using Xunit;
using StateSmithCli.Create;

namespace StateSmithCliTest;

public class NugetTest
{
    [Fact]
    public void Test1()
    {
        NugetVersionGrabber.RunAsync().Wait();
    }
}
