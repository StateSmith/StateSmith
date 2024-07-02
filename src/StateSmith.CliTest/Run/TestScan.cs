using FluentAssertions;
using StateSmith.Cli.Run;
using Xunit;

namespace StateSmith.CliTest.Run;

public class TestScan
{
    readonly string examples1Dir = ExamplesHelper.GetExamplesDir() + "/1";

    [Fact]
    public void RegexTest()
    {
        SsCsxFilter.IsTargetScriptContent("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.9.7-alpha"
            SmRunner runner = new(diagramPath: "MySm.plantuml", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();
            
            """).Should().BeTrue();

        SsCsxFilter.IsTargetScriptContent("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith.Cli, 0.9.7-alpha"
            SmRunner runner = new(diagramPath: "MySm.plantuml", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();
            
            """).Should().BeFalse();
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/341
    /// </summary>
    [Fact]
    public void IntegrationTest_BrokenSvg_341()
    {
        SsCsxDiagramFileFinder finder = new();
        finder.AddDefaultIncludePatternIfNone();
        finder.SetAsRecursive();

        var scanResults = finder.Scan(examples1Dir);
        scanResults.brokenDrawioSvgFiles.Should().BeEquivalentTo(
            "broken-svg.drawio.svg"
        );
    }

    [Fact]
    public void IntegrationTest()
    {
        SsCsxDiagramFileFinder finder = new();
        finder.AddDefaultIncludePatternIfNone();
        finder.AddExcludePattern("a/a3");

        var scanResults = finder.Scan(examples1Dir);
        scanResults.targetCsxFiles.Should().BeEquivalentTo(
            "yes.csx"
        );

        scanResults.targetDiagramFiles.Should().BeEquivalentTo(
            "yes1.plantuml",
            "yes2.plantuml",
            "DiagOnlySm.plantuml"
        );

        scanResults.ignoredFiles.Should().BeEquivalentTo(
        );

        scanResults.nonMatchingFiles.Should().BeEquivalentTo(
        );
    }

    [Fact]
    public void IntegrationTestRecursive()
    {
        SsCsxDiagramFileFinder finder = new();
        finder.AddExcludePattern("a/a3");
        finder.SetAsRecursive();

        var scanResults = finder.Scan(examples1Dir);
        scanResults.targetCsxFiles.Should().BeEquivalentTo(
            "a/a1/yes-a1a.csx",
            "a/a1/yes-a1b.csx",
            "yes.csx"
        );

        scanResults.targetDiagramFiles.Should().BeEquivalentTo(
            "yes1.plantuml",
            "yes2.plantuml",
            "DiagOnlySm.plantuml",
            "a/a1/a1a.plantuml",
            "a/a1/a1b.drawio.svg",
            "a/a1/a1c.plantuml",
            "a/a1/a1-diagram-lang.plantuml"
        );

        scanResults.ignoredFiles.Should().BeEquivalentTo(
            "a/a2/has-inline-ignore.plantuml",
            "a/a2/has-inline-ignore.drawio.svg",
            "a/a2/ignored-a2a.csx"
        );

        scanResults.nonMatchingFiles.Should().BeEquivalentTo(
            "a/a2/missing-sm-name.plantuml",
            "a/a2/no-ss-match.drawio.svg",
            "b/b-non-ss.csx"
        );
    }
}
