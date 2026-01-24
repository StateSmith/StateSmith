#nullable enable

using FluentAssertions;
using StateSmith.Input.Settings;
using StateSmith.Output.UserConfig;
using StateSmith.Output.UserConfig.AutoVars;
using StateSmith.Runner;
using System;
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

            # blah blah
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

        // base
        TestHelper.ExpectPropertyCount<IRenderConfig>(8, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigBaseVars>(8, because: "test need updating");
        // c
        TestHelper.ExpectPropertyCount<IRenderConfigC>(14, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigCVars>(14, because: "test need updating");
        // c#
        TestHelper.ExpectPropertyCount<IRenderConfigCSharp>(6, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigCSharpVars>(6, because: "test need updating");
        // js
        TestHelper.ExpectPropertyCount<IRenderConfigJavaScript>(4, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigJavaScriptVars>(4, because: "test need updating");
        // ts
        TestHelper.ExpectPropertyCount<IRenderConfigTypeScript>(3, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigTypeScriptVars>(3, because: "test need updating");
        // cpp
        TestHelper.ExpectPropertyCount<IRenderConfigCpp>(14, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigCppVars>(14, because: "test need updating");
        // python
        TestHelper.ExpectPropertyCount<IRenderConfigPython>(3, because: "test need updating");
        TestHelper.ExpectFieldCount<RenderConfigPythonVars>(3, because: "test need updating");
        // assert only 8 transpiler IDs
        Enum.GetValues(typeof(TranspilerId)).Length.Should().Be(8 + 2, because: "Needs to be updated for new language"); // +2 for NotYetSet and Default

        var toml = """"
            ############Render Config Settings ##############

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                """
            AutoExpandedVars = ""
            VariableDeclarations = ""
            DefaultVarExpTemplate = ""
            DefaultFuncExpTemplate = ""
            DefaultAnyExpTemplate = ""
            TriggerMap = ""
            EventCommaList = "" # not used yet

            [RenderConfig.C]
            HFileTop = ""
            HFileTopPostIncludeGuard = ""
            HFileIncludes = "#include <stdlib.h>"
            HFileUseExternC = true
            HFileBottomPreIncludeGuard = ""
            HFileBottom = ""
            CFileTop = ""
            CFileIncludes = """#include "some_header.h" """
            CFileBottom = ""
            CFileExtension = ".inc"
            HFileExtension = ".hpp"
            CEnumDeclarer = "typedef enum __attribute__((packed)) {enumName}"
            UseStdBool = false
            IncludeGuardLabel = "MY_HEADER_H"

            [RenderConfig.Cpp]
            HFileTop = ""
            IncludeGuardLabel = ""
            HFileTopPostIncludeGuard = ""
            HFileIncludes = ""
            HFileBottomPreIncludeGuard = ""
            HFileBottom = ""
            CFileTop = ""
            CFileIncludes = ""
            CFileBottom = ""
            CFileExtension = ".cpp"
            HFileExtension = ".hpp"
            NameSpace = ""
            BaseClassCode = ""
            ClassCode = ""

            [RenderConfig.CSharp]
            NameSpace = ""
            Usings = ""
            ClassCode = ""
            BaseList = "MyUserBaseClass, IMyOtherUserInterface"
            UseNullable = false
            UsePartialClass = false

            [RenderConfig.JavaScript]
            ClassCode = ""
            ExtendsSuperClass = "MyUserBaseClass"
            UseExportOnClass = true
            PrivatePrefix = "_"

            [RenderConfig.TypeScript]
            ClassCode = "// some class code"
            Implements = "IRocketSm"
            Extends = "RocketSmBase"

            [RenderConfig.Java]
            Package = ""
            Imports = ""
            Extends = ""
            Implements = ""
            ClassCode = ""

            ############SmRunner.Settings ###############

            [SmRunnerSettings]
            outputDirectory = "./gen"
            outputCodeGenTimestamp = true
            outputStateSmithVersionInfo = false
            propagateExceptions = true
            dumpErrorsToFile = true

            [SmRunnerSettings.smDesignDescriber]
            enabled = true
            outputDirectory = ".."
            outputAncestorHandlers = true

            [SmRunnerSettings.smDesignDescriber.outputSections]
            beforeTransformations = false
            afterTransformations  = true

            [SmRunnerSettings.algoBalanced1]
            outputEventIdToStringFunction = false
            outputStateIdToStringFunction = false

            [SmRunnerSettings.simulation]
            enableGeneration = true
            outputDirectory = ".."
            outputFileNamePostfix = ".sim.html"

            [RenderConfig.Python]
            Imports = """
                # from some_module import some_function
                """
            Extends = "MyUserBaseClass"
            ClassCode = """
                # Add custom code here to inject into the generated class.
                # Inheritance or composition might be a better choice.
                """
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
