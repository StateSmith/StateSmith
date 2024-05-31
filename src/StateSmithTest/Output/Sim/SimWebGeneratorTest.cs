using StateSmith.Common;
using StateSmith.Output.Sim;
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
        GenerateForDiagram(diagramName: "LightSm.drawio.svg");
    }

    [Fact]
    public void LightSm2()
    {
        GenerateForDiagram(diagramName: "LightSm2.drawio.svg");
    }

    [Fact]
    public void PlantEx1()
    {
        GenerateForDiagram(diagramName: "PlantEx1.puml");
    }

    private void GenerateForDiagram(string diagramName)
    {
        SimWebGenerator generator = new(diagramPath: diagramDirPath + diagramName, outputDir: diagramDirPath);
        generator.RunnerSettings.propagateExceptions = true;
        generator.RunnerSettings.outputStateSmithVersionInfo = false; // avoid git noise
        generator.Generate();
    }
}
