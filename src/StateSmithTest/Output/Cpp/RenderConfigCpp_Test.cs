using Xunit;
using FluentAssertions;
using System.Linq;

namespace StateSmithTest.Output.Cpp;

public class RenderConfigCpp_Test
{
    /// <summary>
    /// Tests that .c and .h sections are rendered in the correct order.
    /// Related: https://github.com/StateSmith/StateSmith/issues/385
    /// </summary>
    [Fact]
    public void SectionsOrder_IncludeGuard_385()
    {
        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
                SmRunnerSettings.transpilerId = "Cpp"

                [RenderConfig]
                FileTop = "// FileTop"

                [RenderConfig.Cpp]
                HFileTop = "// HFileTop"
                IncludeGuardLabel = "MY_INCLUDE_GUARD_H"
                HFileTopPostIncludeGuard = "// HFileTopPostIncludeGuard"
                HFileIncludes = "// HFileIncludes"
                HFileBottomPreIncludeGuard = "// HFileBottomPreIncludeGuard"
                HFileBottom = "// HFileBottom"
                CFileTop = "// CFileTop"
                CFileIncludes = "// CFileIncludes"
                CFileBottom = "// CFileBottom"
                HFileExtension = ".hh"
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
            var hCode = fakeFs.GetCapturesForFileName("ExampleSm.hh").Single().code.ConvertLineEndingsToN();

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

    }

    /// <summary>
    /// Tests that .c and .h sections are rendered in the correct order.
    /// Related: https://github.com/StateSmith/StateSmith/issues/385
    /// </summary>
    [Fact]
    public void SectionsOrder_PragmaOnce_385()
    {
        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
                SmRunnerSettings.transpilerId = "Cpp"

                [RenderConfig]
                FileTop = "// FileTop"

                [RenderConfig.Cpp]
                HFileTop = "// HFileTop"
                HFileTopPostIncludeGuard = "// HFileTopPostIncludeGuard"
                HFileIncludes = "// HFileIncludes"
                HFileBottomPreIncludeGuard = "// HFileBottomPreIncludeGuard"
                HFileBottom = "// HFileBottom"
                CFileTop = "// CFileTop"
                CFileIncludes = "// CFileIncludes"
                CFileBottom = "// CFileBottom"
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
    /// https://github.com/StateSmith/StateSmith/issues/427
    /// </summary>
    [Fact]
    public void BaseClassOnlyForTopSmClass_427()
    {
        var plantUmlText = """"
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
            [RenderConfig]
            AutoExpandedVars = """
                int count;
            """
            
            [RenderConfig.Cpp]
            BaseClassCode = "public LightController"
            HFileExtension = ".hpp"
            
            [SmRunnerSettings]
            transpilerId = "Cpp"
            '/
            @enduml
            """";

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);
        var code = fakeFs.GetCapturesForFileName("ExampleSm.hpp").Single().code;

        // use regex to make white space insensitive
        code.Should().NotMatchRegex("""(?x) class \s+ Vars \s+ : \s+ public \s+ LightController""");
    }
}
