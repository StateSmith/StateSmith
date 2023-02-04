using Xunit;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.Linq;

namespace StateSmithTest;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/23
/// </summary>
public class DiagramEmbeddedRenderConfigTests
{
    readonly InputSmBuilder runner = new();

    [Fact]
    public void ProperConversionToVertices()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "RenderConfig1.drawio";
        runner.ConvertDiagramFileToSmVertices(filePath);

        var renderConfig = (RenderConfigVertex)runner.diagramToSmConverter.rootVertices[1];

        RenderConfigOptionVertex vertex;

        vertex = GetOptionVertex(renderConfig, "HFileIncludes");
        vertex.value.ShouldBeShowDiff("""
            // Some user .h file comment...
            """);

        vertex = GetOptionVertex(renderConfig, "CFileIncludes");
        vertex.value.ShouldBeShowDiff("""
            // Some user .c file comment...
            """);

        vertex = GetOptionVertex(renderConfig, "VariableDeclarations");
        vertex.value.ShouldBeShowDiff("""
            uint8_t count;  // some user description for count field
            """);

        vertex = GetOptionVertex(renderConfig, "AutoExpandedVars");
        vertex.value.ShouldBeShowDiff("""
            uint8_t count_2;  // some user description for count_2 field
            """);
    }

    [Fact]
    public void CopyDataFromDiagramRenderConfig()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "RenderConfig1.drawio";
        runner.ConvertDiagramFileToSmVertices(filePath);
        runner.FinishRunning();
    }

    private static RenderConfigOptionVertex GetOptionVertex(RenderConfigVertex renderConfig, string name)
    {
        return renderConfig.Children.OfType<RenderConfigOptionVertex>().Where(v => v.name == name).Single();
    }
}

