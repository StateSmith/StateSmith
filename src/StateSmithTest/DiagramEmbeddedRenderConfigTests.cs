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
    readonly InputSmBuilder runner = TestHelper.CreateInputSmBuilder();

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
            RenderConfigBaseVars renderConfig = runner.sp.GetServiceOrCreateInstance();

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

            renderConfig.DefaultVarExpTemplate.ShouldBeShowDiff("""
            this.myVars.{AutoNameCopy()}
            """);

            renderConfig.DefaultFuncExpTemplate.ShouldBeShowDiff("""
            this.myFuncs.{AutoNameCopy()}
            """);

            renderConfig.DefaultAnyExpTemplate.ShouldBeShowDiff("""
            this.{AutoNameCopy()}
            """);

            renderConfig.EventCommaList.ShouldBeShowDiff("""

            """);

            renderConfig.TriggerMap.ShouldBeShowDiff("""
                // some trigger mapping...
                """);

            const int expectedOptionCount = 8;
            TestHelper.ExpectFieldCount<RenderConfigBaseVars>(expectedOptionCount, because: "above tests need updating");
            TestHelper.ExpectPropertyCount<IRenderConfig>(expectedOptionCount, because: "above tests need updating");
        }

        {
            RenderConfigCVars renderConfig = runner.sp.GetServiceOrCreateInstance();
            var defaultConfig = new RenderConfigCVars();

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

            renderConfig.HFileExtension.ShouldBeShowDiff("""
            .hpp
            """);

            renderConfig.CFileExtension.ShouldBeShowDiff("""
            .cpp
            """);

            renderConfig.CEnumDeclarer.ShouldBeShowDiff("""
            typedef enum __attribute__((packed)) {enumName}
            """);

            renderConfig.IncludeGuardLabel.ShouldBeShowDiff("""
            ROCKETSM_H
            """);
            defaultConfig.IncludeGuardLabel.Should().Be("");

            renderConfig.UseStdBool.Should().BeFalse();
            defaultConfig.UseStdBool.Should().BeTrue();

            // We aren't going to add new options this way, so we don't need to update this.
            // Use the toml settings instead.
            //const int expectedOptionCount = 9;
            //TestHelper.ExpectFieldCount<RenderConfigCVars>(expectedOptionCount, because: "above tests need updating");
            //TestHelper.ExpectPropertyCount<IRenderConfigC>(expectedOptionCount, because: "above tests need updating");
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
            TestHelper.ExpectFieldCount<RenderConfigCSharpVars>(expectedOptionCount, because: "above tests need updating");
            TestHelper.ExpectPropertyCount<IRenderConfigCSharp>(expectedOptionCount, because: "above tests need updating");
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
            TestHelper.ExpectFieldCount<RenderConfigJavaScriptVars>(expectedOptionCount, because: "above tests need updating");
            TestHelper.ExpectPropertyCount<IRenderConfigJavaScript>(expectedOptionCount, because: "above tests need updating");
        }
    }

    private static ConfigOptionVertex GetOptionVertex(RenderConfigVertex renderConfig, string name)
    {
        return renderConfig.Children.OfType<ConfigOptionVertex>().Where(v => v.name == name).Single();
    }
}

