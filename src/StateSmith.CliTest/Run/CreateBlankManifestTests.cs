using FluentAssertions;
using NSubstitute;
using Spectre.Console.Testing;
using StateSmith.Cli.Manifest;
using StateSmith.Cli.Run;
using StateSmith.Common;
using Xunit;

namespace StateSmith.CliTest.Run;

public class CreateBlankManifestTests
{
    private readonly string dirOrManifestPath = ExamplesHelper.GetExamplesDir() + "/1/";
    private readonly IManifestPersistance mockPersistence = Substitute.For<IManifestPersistance>();
    private readonly TestConsole testConsole = new();
    private readonly RunUi runUi;

    public CreateBlankManifestTests()
    {
        // We can't use an NSubstitute console mock because Spectre.Console uses a lot of extension methods.
        // Instead, we mark the console as interactive so that selection prompts will work.
        testConsole.Interactive();
        runUi = new RunUi(new RunOptions(), testConsole, dirOrManifestPath);
    }

    [Fact]
    public void CreateBlankManifest_NoExisting()
    {
        // Arrange
        mockPersistence.ManifestExists().Returns(false);
        ManifestData? data = null;
        mockPersistence.Write(Arg.Do<ManifestData>(md => data = md), overWrite: true);

        // Act
        runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);

        // Assert
        mockPersistence.Received(1).Write(Arg.Any<ManifestData>(), overWrite: true);
        AssertBlankManifestData(data);
    }

    private static void AssertBlankManifestData(ManifestData? data)
    {
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

    [Fact]
    public void CreateBlankManifest_ExistingOverwrite()
    {
        // Arrange
        mockPersistence.ManifestExists().Returns(true);
        ManifestData? data = null;
        mockPersistence.Write(Arg.Do<ManifestData>(md => data = md), overWrite: true);
        // queue select yes.
        // NOTE! This test is a bit fragile because it depends on the order of the choices.
        // We somewhat mitigate this below by checking the output.
        testConsole.Input.PushKey(System.ConsoleKey.DownArrow);
        testConsole.Input.PushTextWithEnter("");

        // Act
        runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);
        testConsole.Output.Should().Match("*Warning!*no*> yes*");

        // Assert
        mockPersistence.Received(1).Write(Arg.Any<ManifestData>(), overWrite: true);
        AssertBlankManifestData(data);
    }

    [Fact]
    public void CreateBlankManifest_ExistingNoOverwrite()
    {
        // Arrange
        mockPersistence.ManifestExists().Returns(true);
        // queue select no
        // NOTE! This test is a bit fragile because it depends on the order of the choices.
        // We somewhat mitigate this below by checking the output.
        testConsole.Input.PushTextWithEnter("");

        // Act
        runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);
        testConsole.Output.Should().Match("*Warning!*> no*yes*");

        // Assert
        mockPersistence.Received(0).Write(Arg.Any<ManifestData>(), Arg.Any<bool>());
    }

}
