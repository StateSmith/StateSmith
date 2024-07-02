using CommandLine;
using StateSmith.Cli;
using StateSmith.Cli.Run;
using System;
using Xunit;
using StateSmith.Common;
using FluentAssertions;
using StateSmith.Runner;

namespace StateSmith.CliTest;

public class CliArgsParserTest
{
    [Fact]
    public void RunOptions_Defaults()
    {
        var runOptions = Parse<RunOptions>("run");
        runOptions.Here.Should().BeFalse();
        runOptions.Rebuild.Should().BeFalse();
        runOptions.Up.Should().BeFalse();
        runOptions.Recursive.Should().BeFalse();
        runOptions.ExcludePatterns.Should().BeEmpty();
        runOptions.IncludePatterns.Should().BeEmpty();
        runOptions.Menu.Should().BeFalse();
        runOptions.Lang.Should().Be(TranspilerId.NotYetSet);
        runOptions.NoSimGen.Should().BeFalse();
    }

    [Fact]
    public void RunOptions_Here()
    {
        Parse<RunOptions>("run -h").Here.Should().BeTrue();
    }

    [Fact]
    public void RunOptions_LangC99()
    {
        Parse<RunOptions>("run --lang C99").Lang.Should().Be(TranspilerId.C99);
    }

    [Fact]
    public void RunOptions_LangCSharp()
    {
        Parse<RunOptions>("run --lang CSharp").Lang.Should().Be(TranspilerId.CSharp);
    }

    [Fact]
    public void RunOptions_LangJavaScript()
    {
        Parse<RunOptions>("run --lang JavaScript").Lang.Should().Be(TranspilerId.JavaScript);
    }

    [Fact]
    public void RunOptions_NoSimGen()
    {
        Parse<RunOptions>("run --no-sim-gen").NoSimGen.Should().BeTrue();
    }
    
    private static T Parse<T>(string args) where T : class
    {
        return Parse<T>(args.Split(' '));
    }

    private static T Parse<T>(string[] args) where T : class
    {
        var parser = new CliArgsParser();
        var result = parser.Parse(args);

        T? resultOptions = null;
        result.MapResult(
            (T options) =>
            {
                resultOptions = options;
                return 0;
            },
            errs =>
            {
                throw new ArgumentException($"Failed to parse: `{string.Join(" ", args)}`");
            }
        );

        return resultOptions.ThrowIfNull(nameof(T) + "was not parsed!");
    }
}
