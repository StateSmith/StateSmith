// disable this file so that it doesn't prevent tests from building
using FluentAssertions;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System.IO;
using Xunit;

namespace StateSmithTest.SmRunnerTest1;

public class SmRunnerTest
{
    readonly char dirSep = Path.DirectorySeparatorChar;

    /// <summary>
    /// doesn't include trailing directory separator
    /// </summary>
    private string ThisDir { get; init; }

    public SmRunnerTest()
    {
         ThisDir = Path.GetFullPath(GetThisDir());
    }

    [Fact]
    public void Test1()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", enablePDBS: false);
        runner.Settings.DiagramPath.Should().Be(ThisDir + dirSep + "SomeDiagram.drawio");
        runner.Settings.outputDirectory.Should().Be(ThisDir + dirSep);
        runner.Settings.filePathPrintBase.Should().Be(ThisDir + dirSep);
    }

    [Fact]
    public void OutputDirTest0()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", outputDirectory: ThisDir, enablePDBS: false);
        runner.Settings.outputDirectory.Should().Be(ThisDir + dirSep);
    }

    [Fact]
    public void OutputDirTest1()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", outputDirectory: ".", enablePDBS: false);
        runner.Settings.outputDirectory.Should().Be(ThisDir + dirSep + "." + dirSep);
    }

    [Fact]
    public void OutputDirTest2()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", outputDirectory: "..", enablePDBS: false);
        runner.Settings.outputDirectory.Should().Be(ThisDir + dirSep + ".." + dirSep);
    }

    [Fact]
    public void PrintBaseDir0()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", enablePDBS: false);
        runner.Settings.filePathPrintBase = ThisDir;
        runner.PrepareBeforeRun();
        runner.Settings.filePathPrintBase.Should().Be(ThisDir + dirSep);
    }

    [Fact]
    public void PrintBaseDir1()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", enablePDBS: false);
        runner.Settings.filePathPrintBase = ".";
        runner.PrepareBeforeRun();
        runner.Settings.filePathPrintBase.Should().Be(Path.GetFullPath(GetThisDir()) + dirSep + "." + dirSep);
    }

    [Fact]
    public void PrintBaseDir2()
    {
        var runner = new SmRunner(diagramPath: "SomeDiagram.drawio", enablePDBS: false);
        runner.Settings.filePathPrintBase = "..";
        runner.PrepareBeforeRun();
        runner.Settings.filePathPrintBase.Should().Be(Path.GetFullPath(GetThisDir()) + dirSep + ".." + dirSep);
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

