#nullable enable

using FluentAssertions;
using StateSmith.Input.Settings;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using Xunit;

namespace StateSmithTest.Output.UserConfig;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/335
/// </summary>
public class TomlConfigTest_335
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
    public void TomlReaderTest()
    {
        RenderConfigAllVars renderConfigAllVars = new();
        RunnerSettings smRunnerSettings = new("");
        var reader = new TomlReader(renderConfigAllVars, smRunnerSettings);

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

    /// <summary>
    /// Important because reflection is used for mapping data to the RenderConfigAllVars class.
    /// We don't want an accidental change to the RenderConfigAllVars class to break the TomlReader.
    /// </summary>
    [Fact]
    public void TestReflectionMapping()
    {
        RenderConfigAllVars renderConfigAllVars = new();
        RunnerSettings smRunnerSettings = new("");
        var reader = new TomlReader(renderConfigAllVars, smRunnerSettings);

        var toml = """"
            [RenderConfig]
            FileTop = "// My file top!"
            AutoExpandedVars = "int count1;"
            # EventCommaList = "event1, event2, event3"   # ignored for now as not used
            VariableDeclarations = "int count2;"
            TriggerMap = ""

            [RenderConfig.C]
            HFileTop = ""
            HFileIncludes = ""
            CFileTop = ""
            CFileIncludes = ""
            CFileExtension = ".c"
            HFileExtension = ".h"

            [RenderConfig.CSharp]
            NameSpace = ""
            Usings = ""
            ClassCode = ""
            BaseList = ""
            UseNullable = true
            UsePartialClass = true
            
            [RenderConfig.JavaScript]
            ExtendsSuperClass = ""
            ClassCode = ""
            UseExportOnClass = false
            PrivatePrefix = "#"
            
            # TODO - consider which of below are important
            [SmRunnerSettings]
            outputStateSmithVersionInfo = false
            transpilerId = "C99"
            algorithmId = "Balanced1"
            simulation = { enableGeneration = true }    # an inline table
            """";

        reader.Read(toml);
        // ignore checking values. we just want to make sure no exceptions are thrown.
    }

    [Fact]
    public void DrawioIntegrationTest()
    {
        var (smRunner, files) = TestHelper.CaptureSmRun(diagramPath: "TomlConfig1.drawio");
        smRunner.Settings.outputStateSmithVersionInfo.Should().BeFalse();

        var hhFile = files.GetCapturesForFileName("TomlConfigEx.hh").Single();
        var incFile = files.GetCapturesForFileName("TomlConfigEx.inc").Single();

        hhFile.code.Should().Contain("// TEST-FILE-TOP");
        incFile.code.Should().Contain("// TEST-FILE-TOP");

        hhFile.code.Should().Contain("// TEST-H-FILE-TOP");
        incFile.code.Should().Contain("// TEST-C-FILE-TOP");

        incFile.code.Should().NotContain("// TEST-H-FILE-TOP");
        hhFile.code.Should().NotContain("// TEST-C-FILE-TOP");
    }

    [Fact]
    public void PlantumlIntegrationTest()
    {
        var (smRunner, files) = TestHelper.CaptureSmRun(diagramPath: "TomlConfig1.plantuml");
        smRunner.Settings.outputStateSmithVersionInfo.Should().BeFalse();

        var hhFile = files.GetCapturesForFileName("TomlConfigEx.hh").Single();
        var incFile = files.GetCapturesForFileName("TomlConfigEx.inc").Single();

        hhFile.code.Should().Contain("// TEST-FILE-TOP");
        incFile.code.Should().Contain("// TEST-FILE-TOP");

        hhFile.code.Should().Contain("// TEST-H-FILE-TOP");
        incFile.code.Should().Contain("// TEST-C-FILE-TOP");

        incFile.code.Should().NotContain("// TEST-H-FILE-TOP");
        hhFile.code.Should().NotContain("// TEST-C-FILE-TOP");
    }
}
