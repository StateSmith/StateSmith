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
            RenderConfigCVars renderConfig = runner.sp.GetServiceOrCreateInstance();

            renderConfig.HFileIncludes.ShouldBeShowDiff("""
            // top level - HFileIncludes
            // sm level - HFileIncludes
            """);

            renderConfig.CFileIncludes.ShouldBeShowDiff("""
            // top level - CFileIncludes
            // sm level - CFileIncludes
            """);

            renderConfig.HFileTop.ShouldBeShowDiff("""
            // top level - HFileTop
            // sm level - HFileTop
            """);

            renderConfig.CFileTop.ShouldBeShowDiff("""
            // top level - CFileTop
            // sm level - CFileTop
            """);

            const int expectedOptionCount = 4;
            GetTypeFields<RenderConfigCVars>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
            GetTypeProperties<IRenderConfigC>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
        }

        {
            var renderConfig = runner.sp.GetInstanceOf<RenderConfigCSharpVars>();
            var defaultConfig = new RenderConfigCSharpVars();

            renderConfig.Usings.ShouldBeShowDiff("""
            // top level - Usings
            // sm level - Usings
            """);

            renderConfig.NameSpace.ShouldBeShowDiff("""
            // top level - NameSpace
            // sm level - NameSpace
            """);

            renderConfig.ClassCode.ShouldBeShowDiff("""
            // sm level - CSharpClassCode
            """);

            renderConfig.BaseList.ShouldBeShowDiff("""
                SomeClass, ISomeInterface
                """);

            renderConfig.UseNullable.Should().BeFalse();
            defaultConfig.UseNullable.Should().BeTrue();
            renderConfig.UsePartialClass.Should().BeFalse();
            defaultConfig.UsePartialClass.Should().BeTrue();

            const int expectedOptionCount = 6;
            GetTypeFields<RenderConfigCSharpVars>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
            GetTypeProperties<IRenderConfigCSharp>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
        }

        {
            var renderConfig = runner.sp.GetInstanceOf<RenderConfigJavaScriptVars>();
            var defaultConfig = new RenderConfigJavaScriptVars();

            renderConfig.ExtendsSuperClass.ShouldBeShowDiff("""
                sm level - ExtendsSuperClass
                """);

            renderConfig.ClassCode.ShouldBeShowDiff("""
                sm level - ClassCode
                """);

            renderConfig.UseExportOnClass.Should().BeTrue();
            defaultConfig.UseExportOnClass.Should().BeFalse();

            renderConfig.PrivatePrefix.ShouldBeShowDiff("""
                sm level - PrivatePrefix
                """);

            const int expectedOptionCount = 4;
            GetTypeFields<RenderConfigJavaScriptVars>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
            GetTypeProperties<IRenderConfigJavaScript>().Length.Should().Be(expectedOptionCount, because: "above tests need updating");
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

