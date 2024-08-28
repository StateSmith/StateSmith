using Xunit;
using FluentAssertions;
using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmithTest.Output.C99;

public class RenderConfigC_Test
{
    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/385
    /// </summary>
    [Fact]
    public void IntegrationTest_IncludeGuard_385()
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
        TestHelper.RunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

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
    /// https://github.com/StateSmith/StateSmith/issues/385
    /// </summary>
    [Fact]
    public void IntegrationTest_PragmaOnce_385()
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
        TestHelper.RunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

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
}
