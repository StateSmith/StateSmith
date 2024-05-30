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
        SimWebGenerator generator = new(diagramPath: diagramDirPath + "LightSm.drawio.svg", outputDir: diagramDirPath);
        generator.Generate();
    }
}
