using FluentAssertions;
using StateSmith.Cli.Run;
using StateSmith.Common;
using Xunit;

namespace StateSmith.CliTest.Run;

public class TestScan
{
    [Fact]
    public void RegexTest()
    {
        SsCsxFileFinder.IsTargetScriptContent("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.9.7-alpha"
            SmRunner runner = new(diagramPath: "MySm.plantuml", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();
            
            """).Should().BeTrue();

        SsCsxFileFinder.IsTargetScriptContent("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.9.7-alpha"
            //<statesmith.cli-ignore-this-file>
            SmRunner runner = new(diagramPath: "MySm.plantuml", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();
            
            """).Should().BeFalse();
    }

    [Fact]
    public void IntegrationTest()
    {
        SsCsxFileFinder finder = new();

        finder.AddExcludePattern("a/a3");

        var found = finder.Scan(TestHelper.GetThisDir() + "/examples/1");
        found.Should().BeEquivalentTo(
            "a/a1/yes-a1a.csx",
            "a/a1/yes-a1b.csx",
            "yes.csx"
        );
    }
}
