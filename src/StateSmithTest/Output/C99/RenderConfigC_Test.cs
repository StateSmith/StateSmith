using Xunit;
using FluentAssertions;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace StateSmithTest.Output.C99;

public class RenderConfigC_Test
{
    /// <summary>
    /// Tests that .c and .h sections are rendered in the correct order.
    /// https://github.com/StateSmith/StateSmith/issues/385
    /// </summary>
    [Fact]
    public void SectionsOrder_IncludeGuard_385()
    {
        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
                SmRunnerSettings.transpilerId = "C99"

                [RenderConfig]
                FileTop = "// FileTop"

                [RenderConfig.C]
                HFileTop = "// HFileTop"
                IncludeGuardLabel = "MY_INCLUDE_GUARD_H"
                HFileTopPostIncludeGuard = "// HFileTopPostIncludeGuard"
                HFileIncludes = "// HFileIncludes"
                HFileBottomPreIncludeGuard = "// HFileBottomPreIncludeGuard"
                HFileBottom = "// HFileBottom"
                CFileTop = "// CFileTop"
                CFileIncludes = "// CFileIncludes"
                CFileBottom = "// CFileBottom"
                CFileExtension = ".cpp"
                HFileExtension = ".hpp"
            '/
            @enduml
            """";

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

        // helper function
        static int ExpectNext(string haystack, int startIndex, string expected)
        {
            // search for expected from start index
            int index = haystack.IndexOf(expected, startIndex);
            index.Should().BeGreaterOrEqualTo(0, $"Expected to find '{expected}' in '{haystack}'");
            return index + expected.Length - 1; // -1 because we don't want to consume trailing line ending for next expectation
        }

        // test H file
        {
            var hCode = fakeFs.GetCapturesForFileName("ExampleSm.hpp").Single().code.ConvertLineEndingsToN();

            int index = 0;
            index = ExpectNext(hCode, index, "\n// FileTop\n");
            index = ExpectNext(hCode, index, "\n// HFileTop\n");
            index = ExpectNext(hCode, index, "\n#ifndef MY_INCLUDE_GUARD_H\n");
            index = ExpectNext(hCode, index, "\n#define MY_INCLUDE_GUARD_H\n");
            index = ExpectNext(hCode, index, "\n// HFileTopPostIncludeGuard\n");
            index = ExpectNext(hCode, index, "\n#include <stdint.h>\n"); // auto included by StateSmith
            index = ExpectNext(hCode, index, "\n// HFileIncludes\n");
            index = ExpectNext(hCode, index, "\n// HFileBottomPreIncludeGuard\n");
            index = ExpectNext(hCode, index, "\n#endif // MY_INCLUDE_GUARD_H\n");
            index = ExpectNext(hCode, index, "\n// HFileBottom\n");
        }

        // test C file
        {
            var cCode = fakeFs.GetCapturesForFileName("ExampleSm.cpp").Single().code.ConvertLineEndingsToN();

            int index = 0;
            index = ExpectNext(cCode, index, "\n// FileTop\n");
            index = ExpectNext(cCode, index, "\n// CFileTop\n");
            index = ExpectNext(cCode, index, "\n#include \"ExampleSm.hpp\"\n"); // auto included by StateSmith
            index = ExpectNext(cCode, index, "\n#include <stdbool.h> ");        // auto included by StateSmith
            index = ExpectNext(cCode, index, "\n#include <string.h> ");         // auto included by StateSmith
            index = ExpectNext(cCode, index, "\n// CFileIncludes\n");
            index = ExpectNext(cCode, index, "\n// CFileBottom\n");
        }
    }

    /// <summary>
    /// Tests that .c and .h sections are rendered in the correct order.
    /// https://github.com/StateSmith/StateSmith/issues/385
    /// </summary>
    [Fact]
    public void SectionsOrder_PragmaOnce_385()
    {
        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
                SmRunnerSettings.transpilerId = "C99"

                [RenderConfig]
                FileTop = "// FileTop"

                [RenderConfig.C]
                HFileTop = "// HFileTop"
                HFileTopPostIncludeGuard = "// HFileTopPostIncludeGuard"
                HFileIncludes = "// HFileIncludes"
                HFileBottomPreIncludeGuard = "// HFileBottomPreIncludeGuard"
                HFileBottom = "// HFileBottom"
                CFileTop = "// CFileTop"
                CFileIncludes = "// CFileIncludes"
                CFileBottom = "// CFileBottom"
                CFileExtension = ".cpp"
                HFileExtension = ".hpp"
            '/
            @enduml
            """";

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

        // helper function
        static int ExpectNext(string haystack, int startIndex, string expected)
        {
            // search for expected from start index
            int index = haystack.IndexOf(expected, startIndex);
            index.Should().BeGreaterOrEqualTo(0, $"Expected to find '{expected}' in '{haystack}'");
            return index + expected.Length - 1; // -1 because we don't want to consume trailing line ending for next expectation
        }

        // test H file
        {
            var hCode = fakeFs.GetCapturesForFileName("ExampleSm.hpp").Single().code.ConvertLineEndingsToN();

            int index = 0;
            index = ExpectNext(hCode, index, "\n// FileTop\n");
            index = ExpectNext(hCode, index, "\n// HFileTop\n");
            index = ExpectNext(hCode, index, "\n#pragma once ");
            index = ExpectNext(hCode, index, "\n// HFileTopPostIncludeGuard\n");
            index = ExpectNext(hCode, index, "\n#include <stdint.h>\n"); // auto included by StateSmith
            index = ExpectNext(hCode, index, "\n// HFileIncludes\n");
            index = ExpectNext(hCode, index, "\n// HFileBottomPreIncludeGuard\n");
            index = ExpectNext(hCode, index, "\n// HFileBottom\n");
        }

        // c file isn't affected by pragma once so we don't need to test it here (see other test)
    }

    /// <summary>
    /// RenderConfig.C.UseStdBool
    /// https://github.com/StateSmith/StateSmith/pull/376
    /// </summary>
    [Fact]
    public void Disable_UseStdBool_376()
    {
        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1
            s1 : EV1 / consume_event = false; /' to ensure `consume_event` generated '/

            /'! $CONFIG: toml
                SmRunnerSettings.transpilerId = "C99"
                RenderConfig.C.UseStdBool = false
            '/
            @enduml
            """";

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

        var cCode = fakeFs.GetCapturesForFileName("ExampleSm.c").Single().code.ConvertLineEndingsToN();
        cCode.Should().NotContain("#include <stdbool.h>");
        cCode.Should().NotContain("bool consume_event = ");
        cCode.Should().Contain("int consume_event = ");
    }

    /// <summary>
    /// RenderConfig.C.HFileUseExternC
    /// https://github.com/StateSmith/StateSmith/pull/435
    /// </summary>
    [Fact]
    public void HFileUseExternC_435()
    {
        const string moduleName = "ExampleSm";
        const string hFileName = moduleName + ".h";

        var outDir = TestHelper.GetThisDir() + "/HFileUseExternC_435";
        if (Directory.Exists(outDir))
        {
            Directory.Delete(outDir, recursive: true);
        }
        Directory.CreateDirectory(outDir);

        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1
            s1 : EV1 / consume_event = false; /' to ensure `consume_event` generated '/

            /'! $CONFIG: toml
                SmRunnerSettings.transpilerId = "C99"
                RenderConfig.C.HFileUseExternC = false
            '/
            @enduml
            """";

        bool Matcher(string code)
        {
            bool matches = true;

            matches &= Regex.Match(code, """
                (?xm)
                ^   [#]ifdef [ ]+ __cplusplus \s* \n+
                ^   extern [ ]+ "C" \s* [{] \s* \n+
                ^   [#]endif \s* \n+
                """).Success;

            matches &= Regex.Match(code, """
                (?xm)
                ^   [#]ifdef [ ]+ __cplusplus \s* \n+
                ^   [}] [ ]* // [ ]* extern [ ]+ "C" \s* \n+
                ^   [#]endif \s* \n+
                """).Success;

            return matches;
        }

        // disabled
        {
            var fakeFs = new CapturingCodeFileWriter();
            TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

            var code = fakeFs.GetCapturesForFileName(hFileName).Single().code.ConvertLineEndingsToN();
            Matcher(code).Should().BeFalse();
        }

        // enabled
        {
            plantUmlText = plantUmlText.Replace("HFileUseExternC = false", "HFileUseExternC = true");
            var fakeFs = new CapturingCodeFileWriter();
            TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

            var code = fakeFs.GetCapturesForFileName(hFileName).Single().code.ConvertLineEndingsToN();
            Matcher(code).Should().BeTrue();

            File.WriteAllText(outDir + "/" + hFileName, code);
        }
    }
}
