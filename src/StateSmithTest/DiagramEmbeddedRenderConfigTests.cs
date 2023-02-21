using Xunit;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.Linq;
using StateSmith.Output.UserConfig;
using FluentAssertions;
using System.Reflection;

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

        ConfigOptionVertex vertex;

        vertex = GetOptionVertex(renderConfig, "HFileIncludes");
        vertex.value.ShouldBeShowDiff("""
            // top level - HFileIncludes
            """);

        vertex = GetOptionVertex(renderConfig, "CFileIncludes");
        vertex.value.ShouldBeShowDiff("""
            // top level - CFileIncludes
            """);

        vertex = GetOptionVertex(renderConfig, "VariableDeclarations");
        vertex.value.ShouldBeShowDiff("""
            int top_level; // top level - VariableDeclarations
            """);

        // below test tests other fields
    }

    [Fact]
    public void CopyDataFromDiagramRenderConfig()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "RenderConfig1.drawio";
        runner.ConvertDiagramFileToSmVertices(filePath);
        runner.FinishRunning();

        RenderConfigC renderConfig = runner.sp.GetServiceOrCreateInstance();

        renderConfig.HFileIncludes.ShouldBeShowDiff("""
            // top level - HFileIncludes
            // sm level - HFileIncludes
            """);

        renderConfig.CFileIncludes.ShouldBeShowDiff("""
            // top level - CFileIncludes
            // sm level - CFileIncludes
            """);

        // FIXME - render configs should not inherit from eachother
        // a code generator may request the general one and the specific one
        renderConfig.VariableDeclarations.ShouldBeShowDiff("""
            int top_level; // top level - VariableDeclarations
            int sm_level; // sm level - VariableDeclarations
            """);

        renderConfig.FileTop.ShouldBeShowDiff("""
            // top level - FileTop
            // sm level - FileTop
            """);

        renderConfig.HFileTop.ShouldBeShowDiff("""
            // top level - HFileTop
            // sm level - HFileTop
            """);

        renderConfig.CFileTop.ShouldBeShowDiff("""
            // top level - CFileTop
            // sm level - CFileTop
            """);

        renderConfig.AutoExpandedVars.ShouldBeShowDiff("""
            // top level - AutoExpandedVars
            // sm level - AutoExpandedVars
            """);

        renderConfig.EventCommaList.ShouldBeShowDiff("""

            """);

        const int expectedOptionCount = 7;
        GetRenderConfigFields().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
        GetIRenderConfigCProperties().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
    }

    private static FieldInfo[] GetRenderConfigFields()
    {
        return typeof(RenderConfigC).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
    }

    private static MethodInfo[] GetIRenderConfigCProperties()
    {
        return typeof(IRenderConfigC).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public);
    }

    private static ConfigOptionVertex GetOptionVertex(RenderConfigVertex renderConfig, string name)
    {
        return renderConfig.Children.OfType<ConfigOptionVertex>().Where(v => v.name == name).Single();
    }
}

