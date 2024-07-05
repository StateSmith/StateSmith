using Antlr4.Runtime.Misc;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Testing;
using StateSmith.Cli;
using StateSmith.Cli.Manifest;
using StateSmith.Cli.Run;
using StateSmith.Common;
using System.Diagnostics;
using Xunit;

namespace StateSmith.CliTest.Run;

public class RunTest
{
    readonly string dirOrManifestPath = ExamplesHelper.GetExamplesDir() + "/1/";

    [Fact]
    public void DebuggerTest()
    {
        if (!Debugger.IsAttached)
            return;

        RunHandler runHandler = new(AnsiConsole.Console, dirOrManifestPath, new(), new(currentDirectory: dirOrManifestPath));
        runHandler.Finder.AddExcludePattern("a/a3");
        runHandler.Finder.SetAsRecursive();
    }

    [Fact]
    public void DebuggerTest2()
    {
        if (!Debugger.IsAttached)
            return;

        //var args = "run -hr --no-csx --propagate-exceptions".Split(' ');
        var args = "run -hr --no-csx".Split(' ');

        var program = new Cli.Program(currentDirectory: dirOrManifestPath);
        TestConsole fakeConsole = new();
        program.ParseCommandsAndRun(args, fakeConsole);
    }

    [Fact]
    public void CreateBlankManifest()
    {
        var manifestPersistence = Substitute.For<IManifestPersistance>();
        RunHandler runHandler = new(AnsiConsole.Console, dirOrManifestPath, new(), new(currentDirectory: dirOrManifestPath), manifestPersistence);
        runHandler.CreateBlankManifest();

        ManifestData? data = null;
        manifestPersistence.Received(1).Write(Arg.Is<ManifestData>(md => Capture(md, out data)), overWrite: true);

        data.ThrowIfNull().RunManifest.IncludePathGlobs.Should().BeEquivalentTo([
                "**/*.csx",
                "**/*.drawio.svg",
                "**/*.drawio",
                "**/*.dio",
                "**/*.plantuml",
                "**/*.puml",
                "**/*.pu",
                "**/*.graphml",
            ]);
    }

    /// <summary>
    /// This is a helper method to get around NSubstitute matcher limitations.
    /// Can't use a generic lambda in the Arg.Is method. Must be an expression.
    /// See https://github.com/nsubstitute/NSubstitute/issues/637
    /// </summary>
    public static bool Capture<T>(T input, out T output)
    {
        output = input;
        return true;
    }
}
