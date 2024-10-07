using FluentAssertions;
using NSubstitute;
using Spectre.Console.Testing;
using StateSmith.Cli.Manifest;
using StateSmith.Cli.Run;
using StateSmith.Cli.Utils;
using StateSmith.CliTest.Utils;
using StateSmith.Common;
using System.Threading.Tasks;
using Xunit;

namespace StateSmith.CliTest.Run;

/// <summary>
/// NOTE! This class has a number of experiments on how best to test the console UI.
/// </summary>
public class CreateBlankManifestTests
{
    private readonly string dirOrManifestPath = ExamplesHelper.GetExamplesDir() + "/1/";

    private const string ManifestWriteSuccessMessage = "Manifest written successfully to statesmith.cli.json";
    private const string OverwriteConfirmationMessage = "Overwrite existing files?";
    private const string NoSelection = "> no";
    private const string YesSelection = "> yes";
    private readonly IManifestPersistance mockPersistence = Substitute.For<IManifestPersistance>();

    public CreateBlankManifestTests()
    {
       
    }

    [Fact]
    public void CreateBlankManifest_NoExisting()
    {
        // Arrange
        RunUi runUi = new(new RunOptions(), new TestConsole(), dirOrManifestPath);

        mockPersistence.ManifestExists().Returns(false);
        ManifestData? data = null;
        mockPersistence.Write(Arg.Do<ManifestData>(md => data = md), overWrite: true);

        // Act
        runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);

        // Assert
        mockPersistence.Received(1).Write(Arg.Any<ManifestData>(), overWrite: true);
        AssertBlankManifestData(data);
    }

    [Fact]
    public void CreateBlankManifest_ExistingOverwrite()
    {
        // Arrange
        TestConsole testConsole = new();
        // We can't use an NSubstitute console mock because Spectre.Console uses a lot of extension methods.
        // Instead, we mark the console as interactive so that selection prompts will work.
        testConsole.Interactive();
        RunUi runUi = new(new RunOptions(), testConsole, dirOrManifestPath);

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

    // Less fragile for menu driving. Probably better to create a menu abstraction instead.
    [Fact]
    public void CreateBlankManifest_ExistingOverwrite_Thread()
    {
        // Arrange
        ManifestData? data = null;
        mockPersistence.Write(Arg.Do<ManifestData>(md => data = md), overWrite: true);

        var liveConsole = new DrivableConsole();
        var runUi = new RunUi(new RunOptions(), liveConsole, dirOrManifestPath);
        mockPersistence.ManifestExists().Returns(true);

        // Act
        var task = new Task(() =>
        {
            runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);
        });
        task.Start();

        liveConsole.WaitForOutputIgnoreCase(OverwriteConfirmationMessage);
        liveConsole.WaitForOutputIgnoreCase(NoSelection);
        liveConsole.ClearOutput();
        liveConsole.SendDownArrow();
        liveConsole.WaitForOutputIgnoreCase(YesSelection);
        liveConsole.SendEnter();

        // Assert
        task.Wait(1000).Should().BeTrue(); // will throw if task had an exception

        liveConsole.Output.Should().Contain(ManifestWriteSuccessMessage);
        mockPersistence.Received(1).Write(Arg.Any<ManifestData>(), true);
        AssertBlankManifestData(data);
    }

    [Fact]
    public void CreateBlankManifest_ExistingOverwrite_Prompter()
    {
        // Arrange
        ManifestData? data = null;
        mockPersistence.Write(Arg.Do<ManifestData>(md => data = md), overWrite: true);

        TestConsole testConsole = new();
        IPrompter prompter = Substitute.For<IPrompter>();
        RunUi runUi = new(new RunOptions(), testConsole, dirOrManifestPath, prompter);
        mockPersistence.ManifestExists().Returns(true);
        prompter.AskForOverwrite().Returns(true);

        // Act
        runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);

        // Assert
        testConsole.Output.Should().Contain(ManifestWriteSuccessMessage);
        mockPersistence.Received(1).Write(Arg.Any<ManifestData>(), true);
        AssertBlankManifestData(data);
    }

    [Fact]
    public void CreateBlankManifest_ExistingNoOverwrite_Prompter()
    {
        // Arrange
        TestConsole testConsole = new();
        IPrompter prompter = Substitute.For<IPrompter>();
        RunUi runUi = new(new RunOptions(), testConsole, dirOrManifestPath, prompter);
        mockPersistence.ManifestExists().Returns(true);
        prompter.AskForOverwrite().Returns(false);

        // Act
        runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);

        // Assert
        mockPersistence.Received(0).Write(Arg.Any<ManifestData>(), Arg.Any<bool>());
        testConsole.Output.Should().NotContain(ManifestWriteSuccessMessage);
    }

    [Fact]
    public void CreateBlankManifest_ExistingNoOverwrite_Thread()
    {
        // Arrange
        var liveConsole = new DrivableConsole();
        var runUi = new RunUi(new RunOptions(), liveConsole, dirOrManifestPath);
        mockPersistence.ManifestExists().Returns(true);

        // Act
        var task = new Task(() =>
        {
            runUi.CreateBlankManifestAskIfOverwrite(mockPersistence);
        });
        task.Start();

        liveConsole.WaitForOutputIgnoreCase(OverwriteConfirmationMessage);
        liveConsole.WaitForOutputIgnoreCase(NoSelection);
        liveConsole.SendEnter();

        // Assert
        task.Wait(1000).Should().BeTrue(); // will throw if task had an exception
        liveConsole.Output.Should().NotContain(ManifestWriteSuccessMessage);
        mockPersistence.Received(0).Write(Arg.Any<ManifestData>(), Arg.Any<bool>());
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
}
