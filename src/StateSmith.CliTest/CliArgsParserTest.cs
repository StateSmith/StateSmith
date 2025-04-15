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

    [Fact]
    public void RunOptions_Files()
    {
        var options = Parse<RunOptions>("run --watch file1.puml file2.plantuml f3-design.drawio");
        // note that we can't test for a quoted file string (like "my file.puml") here as that is handled by the shell, not the parser.

        options.Watch.Should().BeTrue();
        options.SpecificFiles.Should().BeEquivalentTo(["file1.puml", "file2.plantuml", "f3-design.drawio"]);

        // extra space at the end causes an empty string to be added to the list. Maybe won't happen with a real shell.
        options = Parse<RunOptions>("run --watch file1.puml ");
        options.Watch.Should().BeTrue();
        options.SpecificFiles.Should().BeEquivalentTo(["file1.puml", ""]);
    }

    [Fact]
    public void RunOptions_FilesInFrontOfOptions()
    {
        var options = Parse<RunOptions>("run file1.puml file2.plantuml f3-design.drawio --watch");
        options.Watch.Should().BeTrue();
        options.SpecificFiles.Should().BeEquivalentTo(["file1.puml", "file2.plantuml", "f3-design.drawio"]);

        // extra space at the end causes an empty string to be added to the list. Maybe won't happen with a real shell.
        options = Parse<RunOptions>("run file1.puml file2.plantuml f3-design.drawio --watch ");
        options.Watch.Should().BeTrue();
        options.SpecificFiles.Should().BeEquivalentTo(["file1.puml", "file2.plantuml", "f3-design.drawio", ""]); // Note extra blank string
    }

    [Fact]
    public void RunOptions_FilesInMiddleOfOptions()
    {
        var options = Parse<RunOptions>("run file1.puml --watch file2.plantuml");
        options.Watch.Should().BeTrue();
        options.SpecificFiles.Should().BeEquivalentTo(["file1.puml", "file2.plantuml"]);
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
