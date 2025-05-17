using FluentAssertions;
using StateSmith.Cli.Run;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace StateSmith.CliTest.Run;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/428
/// </summary>
public class DiagramRunner_ApplyConfigFiles
{
    private const string Dir1 = "dir1";
    private const string Dir2 = "dir2";

    // The fake current directory for the test. It is a fully qualified path.
    // We can't just use a string like "/home/user/dir" because that is OS dependent and will fail on Windows.
    private static readonly string rootDir = TestHelper.GetThisDir();

    // The fake current directory for the test. It is a fully qualified path.
    private static readonly string curAbsPath = Path.Combine(rootDir, Dir1, Dir2);

    [Fact]
    public void LocalFilesToAbsolute()
    {
        var inputs = new List<string>
        {
            "config1.toml",
            "./config2.toml",
            "./dir3/config3.toml"
        };

        var expected = new List<string>
        {
            Path.Combine(rootDir, Dir1, Dir2, "config1.toml"),
            Path.Combine(rootDir, Dir1, Dir2, "config2.toml"),
            Path.Combine(rootDir, Dir1, Dir2, "dir3", "config3.toml"),
        };

        Validate(inputs, expected);
    }

    [Fact]
    public void RelativeUpToAbsolute()
    {
        var inputs = new List<string>
        {
            "../config1.toml",
            "../../config2.toml"
        };

        var expected = new List<string>
        {
            Path.Combine(rootDir, Dir1, "config1.toml"),
            Path.Combine(rootDir, "config2.toml"),
        };

        Validate(inputs, expected);
    }

    [Fact]
    public void AbsoluteStaysAbsolute()
    {
        var inputs = new List<string>
        {
            Path.Combine(rootDir, "config1.toml"),
            Path.Combine(rootDir, Dir2, "config1.toml"),
        };

        var expected = new List<string>
        {
            Path.Combine(rootDir, "config1.toml"),
            Path.Combine(rootDir, Dir2, "config1.toml"),
        };

        Validate(inputs, expected);
    }

    private static void Validate(List<string> inputs, List<string> expected)
    {
        Assert.True(Path.IsPathFullyQualified(curAbsPath)); // required for testing

        var runHandlerOptions = new RunHandlerOptions(curAbsPath);
        runHandlerOptions.ConfigFiles = inputs;

        var runnerSettings = new Runner.RunnerSettings();
        DiagramRunner.ApplyConfigFiles(runnerSettings, runHandlerOptions);
        runnerSettings.configFiles.Should().BeEquivalentTo(expected);
    }
}
