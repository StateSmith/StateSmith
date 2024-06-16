#nullable enable

using FluentAssertions;
using StateSmith.Input.Settings;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using Tomlyn;
using Tomlyn.Model;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

public class RenderConfigTomlTest
{
    /// <summary>
    /// Test out Tomlyn parsing.
    /// </summary>
    [Fact]
    public void TestParsingToml()
    {
        var toml = """"
            [RenderConfig]
            CFileExtension = ".inc"
            HFileTop = """
            // comment at top 1
            // comment at top 2
            """

            [RenderConfig.CSharp]
            NameSpace = "MyNamespace"
            UseNullable = false
            """";

        var model = Toml.ToModel(toml);
        var RenderConfig = (TomlTable)model["RenderConfig"];

        RenderConfig.Values.Should().HaveCount(3);

        {
            ((string)RenderConfig["CFileExtension"]).Should().Be(".inc");
            ((string)RenderConfig["HFileTop"]).ReplaceLineEndings("\n").ShouldBeShowDiff("// comment at top 1\n// comment at top 2\n");
        }

        {
            var CSharp = (TomlTable)RenderConfig["CSharp"];
            ((string)CSharp["NameSpace"]).Should().Be("MyNamespace");
            ((bool)CSharp["UseNullable"]).Should().BeFalse();
        }
    }

    /// <summary>
    /// Check to see if Tomlyn can parse inline tables.
    /// https://toml.io/en/v1.0.0#inline-table
    /// </summary>
    [Fact]
    public void TestParsingInlineTableToml()
    {
        var toml1 = """"
            name = { first = "Tom", last = "Preston-Werner" }
            point = { x = 1, y = 2 }
            animal = { type.name = "pug" }
            """";

        var model = Toml.ToModel(toml1);
        var name = (TomlTable)model["name"];
        {
            ((string)name["first"]).Should().Be("Tom");
            ((string)name["last"]).Should().Be("Preston-Werner");
        }

        var point = (TomlTable)model["point"];
        {
            ((long)point["x"]).Should().Be(1);
            ((long)point["y"]).Should().Be(2);
        }

        var animal = (TomlTable)model["animal"];
        {
            var type = (TomlTable)animal["type"];
            ((string)type["name"]).Should().Be("pug");
        }
    }

    [Fact]
    public void Test1()
    {
        RenderConfigAllVars renderConfigAllVars = new();
        RunnerSettings smRunnerSettings = new("");
        var reader = new TomlReader(renderConfigAllVars, smRunnerSettings);

        // $StateSmith(toml)
        var toml = """"
            [RenderConfig]
            FileTop = "// My file top!"
            AutoExpandedVars = """
                int count1;
                int count2;
                """

            [RenderConfig.C]
            HFileTop = """
                // comment at top 1
                // comment at top 2
                """
            CFileExtension = ".inc"

            [SmRunnerSettings]
            outputStateSmithVersionInfo = false
            transpilerId = "C99"
            algorithmId = "Balanced1"
            simulation = { enableGeneration = true }    # an inline table
            """";

        reader.Read(toml);

        renderConfigAllVars.Base.FileTop.ShouldBeShowDiff("// My file top!");
        renderConfigAllVars.Base.AutoExpandedVars.ShouldBeShowDiff("int count1;\nint count2;\n");

        renderConfigAllVars.C.HFileTop.ShouldBeShowDiff("// comment at top 1\n// comment at top 2\n");
        renderConfigAllVars.C.CFileExtension.Should().Be(".inc");

        smRunnerSettings.outputStateSmithVersionInfo.Should().BeFalse();
        smRunnerSettings.transpilerId.Should().Be(TranspilerId.C99);
        smRunnerSettings.algorithmId.Should().Be(AlgorithmId.Balanced1);
        smRunnerSettings.simulation.enableGeneration.Should().BeTrue();
    }
}