using StateSmith.Common;
using StateSmith.Output;
using StateSmith.Output.Sim;
using StateSmith.Runner;
using System.IO;
using Xunit;

namespace StateSmithTest.Output.Sim;

/// <summary>
/// These are not proper unit tests. More like manual tests for quickly trying things out.
/// Otherwise, we would need to do full nuget releases to test the web generator changes (way too slow),
/// or create a separate C# solution to test the web generator changes.
/// </summary>
public class SimWebGenerator_IntegrationTests
{
    readonly string diagramDirPath = PathUtils.GetThisDir() + "/diagrams/";

    [Fact]
    public void LightSm()
    {
        GenerateForDiagram(diagramName: "LightSm.drawio.svg", expectedOutputFileName: "LightSm.sim.html");
    }

    [Fact]
    public void LightSm2()
    {
        GenerateForDiagram(diagramName: "LightSm2.drawio.svg", expectedOutputFileName: "LightSm2.sim.html");
    }

    [Fact]
    public void PlantEx1()
    {
        GenerateForDiagram(diagramName: "PlantEx1.puml", expectedOutputFileName: "PlantEx1.sim.html");
    }

    [Fact]
    public void BeadSorter()
    {
        GenerateForDiagram(diagramName: "BeadSorter.drawio.svg", expectedOutputFileName: "BeadSorter.sim.html");
    }

    [Fact]
    public void TriggerMap()
    {
        GenerateForDiagram(diagramName: "TriggerMap.drawio.svg", expectedOutputFileName: "TriggerMap.sim.html");
    }

    /// <summary>
    /// This test shows how a .csx file user can enable the simulation feature.
    /// </summary>
    [Fact]
    public void SmRunner_PlantEx2()
    {
        string expectedOutputFilePath = diagramDirPath + "PlantEx2.sim.html";
        DeleteIfFileExists(expectedOutputFilePath);

        SmRunner smRunner = new(diagramPath: diagramDirPath + "PlantEx2.puml");
        smRunner.Settings.simulation.enableGeneration = true;
        smRunner.Settings.propagateExceptions = true;   // just for testing here
        smRunner.Settings.outputStateSmithVersionInfo = false; // avoid git noise
        smRunner.Run();

        AssertFileExists(expectedOutputFilePath);
    }

    [Fact]
    public void SpaceControlUiSm()
    {
        GenerateForDiagram(diagramName: "SpaceControlUiSm.drawio", expectedOutputFileName: "SpaceControlUiSm.sim.html");
    }
    

    private static void DeleteIfFileExists(string expectedOutputFile)
    {
        if (File.Exists(expectedOutputFile))
        {
            File.Delete(expectedOutputFile);
        }
    }

    private void GenerateForDiagram(string diagramName, string expectedOutputFileName)
    {
        string expectedOutputFilePath = diagramDirPath + expectedOutputFileName;
        DeleteIfFileExists(expectedOutputFilePath);

        CodeFileWriter codeFileWriter = new(consolePrinter: new ConsolePrinter(), pathPrinter: new FilePathPrinter(diagramDirPath));
        SimWebGenerator generator = new(codeFileWriter, new());
        generator.RunnerSettings.propagateExceptions = true;
        generator.RunnerSettings.outputStateSmithVersionInfo = false; // avoid git noise
        generator.Generate(diagramPath: diagramDirPath + diagramName, outputDir: diagramDirPath);

        AssertFileExists(expectedOutputFilePath);
    }

    private static void AssertFileExists(string filePath)
    {
        Assert.True(File.Exists(filePath), $"File does not exist: {filePath}");
    }
}
