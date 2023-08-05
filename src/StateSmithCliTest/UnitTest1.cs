using System;
using Xunit;

using StateSmith.Runner;

namespace StateSmithCliTest;

public class NugetTest
{
    [Fact]
    public void Test1()
    {
        NugetVersionGrabber.RunAsync().Wait();
    }
}
