// disable this file so that it doesn't prevent tests from building
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;
using Xunit;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StateSmithTest.SmRunnerTest1;

public class SmRunnerTest
{
    readonly char dirSep = Path.DirectorySeparatorChar;

    /// <summary>
    /// doesn't include trailing directory separator
    /// </summary>
    private string ThisDir { get; init; }

    private readonly IServiceProvider serviceProvider = RunnerServiceProviderFactory.CreateDefault((services)=>{
        // Disable PreDiagramSettingsReader
        services.RemoveAll<PreDiagramSettingsReader>();
    });

    public SmRunnerTest()
    {
        ThisDir = Path.GetFullPath(GetThisDir());
    }

    [Fact]
    public void Test1()
    {
        SmRunner.Create(diagramPath: "SomeDiagram.drawio", serviceProvider: serviceProvider);
        var settings = serviceProvider.GetRequiredService<RunnerSettings>();
        settings.DiagramPath.Should().Be(ThisDir + dirSep + "SomeDiagram.drawio");
        settings.outputDirectory.Should().Be(ThisDir + dirSep);
        settings.filePathPrintBase.Should().Be(ThisDir + dirSep);
    }

    [Fact]
    public void OutputDirTest0()
    {
        SmRunner.Create(diagramPath: "SomeDiagram.drawio", outputDirectory: ThisDir, serviceProvider: serviceProvider);
        var settings = serviceProvider.GetRequiredService<RunnerSettings>();
        settings.outputDirectory.Should().Be(ThisDir + dirSep);
    }

    [Fact]
    public void OutputDirTest1()
    {
        SmRunner.Create(diagramPath: "SomeDiagram.drawio", outputDirectory: ".", serviceProvider: serviceProvider);
        var settings = serviceProvider.GetRequiredService<RunnerSettings>();
        settings.outputDirectory.Should().Be(ThisDir + dirSep + "." + dirSep);
    }

    [Fact]
    public void OutputDirTest2()
    {
        SmRunner.Create(diagramPath: "SomeDiagram.drawio", outputDirectory: "..", serviceProvider: serviceProvider);
        var settings = serviceProvider.GetRequiredService<RunnerSettings>();
        settings.outputDirectory.Should().Be(ThisDir + dirSep + ".." + dirSep);
    }

    [Fact]
    public void PrintBaseDir0()
    {
        RunnerSettings settings = new()
        {
            DiagramPath = "SomeDiagram.drawio",
            filePathPrintBase = ThisDir
        };
        var runner = SmRunner.Create(settings, serviceProvider: serviceProvider);
        Assert.Throws<FinishedWithFailureException>(() => runner.Run()); // This is because the diagram file does not exist and can be ignored.
        settings.filePathPrintBase.Should().Be(ThisDir + dirSep);
    }

    [Fact]
    public void PrintBaseDir1()
    {
        RunnerSettings settings = new()
        {
            DiagramPath = "SomeDiagram.drawio",
            filePathPrintBase = "."
        };
        var runner = SmRunner.Create(settings, serviceProvider: serviceProvider);
        Assert.Throws<FinishedWithFailureException>(() => runner.Run()); // This is because the diagram file does not exist and can be ignored.
        settings.filePathPrintBase.Should().Be(Path.GetFullPath(GetThisDir()) + dirSep + "." + dirSep);
    }

    [Fact]
    public void PrintBaseDir2()
    {
        RunnerSettings settings = new()
        {
            DiagramPath = "SomeDiagram.drawio",
            filePathPrintBase = ".."
        };
        var runner = SmRunner.Create(settings, serviceProvider: serviceProvider);
        Assert.Throws<FinishedWithFailureException>(() => runner.Run()); // This is because the diagram file does not exist and can be ignored.
        settings.filePathPrintBase.Should().Be(Path.GetFullPath(GetThisDir()) + dirSep + ".." + dirSep);
    }

    // see https://stackoverflow.com/questions/46728845/c-sharp-for-scripting-csx-location-of-script-file
    string GetThisDir([System.Runtime.CompilerServices.CallerFilePath] string fileName = null)
    {
        return Path.GetDirectoryName(fileName);
    }
}


#pragma warning disable IDE1006, CA1050 // ignore C# guidelines for script stuff below

public class MyRenderConfig : IRenderConfig
{
    string IRenderConfig.AutoExpandedVars => @"
        int count;
    ";

    public class Expansions : UserExpansionScriptBase
    {
        string inc_count() => $"{VarsPath}count++";
    }
}

