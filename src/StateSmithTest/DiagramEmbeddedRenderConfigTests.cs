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

        {
            RenderConfigVars renderConfig = runner.sp.GetServiceOrCreateInstance();

            renderConfig.VariableDeclarations.ShouldBeShowDiff("""
            int top_level; // top level - VariableDeclarations
            int sm_level; // sm level - VariableDeclarations
            """);

            renderConfig.FileTop.ShouldBeShowDiff("""
            // top level - FileTop
            // sm level - FileTop
            """);


            renderConfig.AutoExpandedVars.ShouldBeShowDiff("""
            // top level - AutoExpandedVars
            // sm level - AutoExpandedVars
            """);

            renderConfig.EventCommaList.ShouldBeShowDiff("""

            """);

            const int expectedOptionCount = 4;
            GetTypeFields<RenderConfigVars>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
            GetTypeProperties<IRenderConfig>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
        }

        {
            RenderConfigCVars renderConfigC = runner.sp.GetServiceOrCreateInstance();

            renderConfigC.HFileIncludes.ShouldBeShowDiff("""
            // top level - HFileIncludes
            // sm level - HFileIncludes
            """);

            renderConfigC.CFileIncludes.ShouldBeShowDiff("""
            // top level - CFileIncludes
            // sm level - CFileIncludes
            """);

            renderConfigC.HFileTop.ShouldBeShowDiff("""
            // top level - HFileTop
            // sm level - HFileTop
            """);

            renderConfigC.CFileTop.ShouldBeShowDiff("""
            // top level - CFileTop
            // sm level - CFileTop
            """);

            const int expectedOptionCount = 4;
            GetTypeFields<RenderConfigCVars>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
            GetTypeProperties<IRenderConfigC>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
        }

        {
            RenderConfigCSharpVars renderConfigCSharp = runner.sp.GetServiceOrCreateInstance();

            renderConfigCSharp.Usings.ShouldBeShowDiff("""
            // top level - Usings
            // sm level - Usings
            """);

            renderConfigCSharp.NameSpace.ShouldBeShowDiff("""
            // top level - NameSpace
            // sm level - NameSpace
            """);

            renderConfigCSharp.ClassCode.ShouldBeShowDiff("""
            // sm level - CSharpClassCode
            """);

            renderConfigCSharp.UseNullable.Should().BeFalse();
            renderConfigCSharp.UsePartialClass.Should().BeFalse();

            const int expectedOptionCount = 5;
            GetTypeFields<RenderConfigCSharpVars>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
            GetTypeProperties<IRenderConfigCSharp>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
        }
    }

    private static FieldInfo[] GetTypeFields<T>()
    {
        return typeof(T).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
    }

    private static MethodInfo[] GetTypeProperties<T>()
    {
        return typeof(T).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public);
    }

    private static ConfigOptionVertex GetOptionVertex(RenderConfigVertex renderConfig, string name)
    {
        return renderConfig.Children.OfType<ConfigOptionVertex>().Where(v => v.name == name).Single();
    }
}

