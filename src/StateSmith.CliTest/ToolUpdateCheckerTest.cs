using FluentAssertions;
using NuGet.Versioning;
using Spectre.Console.Testing;
using StateSmith.Cli;
using StateSmith.Cli.Data;
using StateSmith.Cli.VersionUtils;
using Xunit;

namespace StateSmith.CliTest;

public class ToolUpdateCheckerTest
{
    private const string StringForNewVersion = "A new version of StateSmith.Cli is available";

    TestConsole console = new();
    SimpleSemVerProvider semVerProvider = new("0.0.0");
    ToolUpdateChecker toolUpdateChecker;

    public ToolUpdateCheckerTest()
    {
        toolUpdateChecker = new(console, new DataPaths(console), semVerProvider);
    }

    [Fact]
    public void ThisAssemblySemVerProvider()
    {
        new ThisAssemblySemVerProvider().GetVersion(); // should not throw
    }

    [Fact]
    public void CheckForUpdates_NewVersion()
    {
        semVerProvider.Set("0.0.0");
        toolUpdateChecker.CheckForUpdates(pauseForKeyboardEnter: false);
        console.Output.Should().Contain(StringForNewVersion);
    }

    [Fact]
    public void CheckForUpdates_NoNewVersion()
    {
        semVerProvider.Set("100000.0.0");
        toolUpdateChecker.CheckForUpdates(pauseForKeyboardEnter: false);
        console.Output.Should().NotContain(StringForNewVersion);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/351
    /// </summary>
    [Fact]
    public void CompareVersions_351()
    {
        SemanticVersion currentVersion = SemanticVersion.Parse("0.1.0");
        SemanticVersion otherVersion = SemanticVersion.Parse("0.1.1");
        MyVersionComparer.IsOtherVersionGreater(currentVersion, otherVersion: otherVersion).Should().BeTrue();
        
        // pre-release versions should not be considered greater
        currentVersion = SemanticVersion.Parse("0.10.0");
        otherVersion = SemanticVersion.Parse("0.10.0-alpha-1");
        MyVersionComparer.IsOtherVersionGreater(currentVersion, otherVersion: otherVersion).Should().BeFalse();
    }
}
