using System;
using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;
using System.Linq;
using System.Collections.Generic;

namespace StateSmithTest;

public static class TestExtensions
{
    public static string ConvertLineEndingsToN(this string code)
    {
        return code.Replace("\r\n", "\n");
    }

    public static string SanitizeCode(this string code)
    {
        return code.Trim().Replace("\r\n", "\n");
    }

    public static void EqualsCode(this string code, string expectedCode)
    {
        if (expectedCode.SanitizeCode() != code.SanitizeCode())
        {
            Console.WriteLine(expectedCode);
            Console.WriteLine(code);
            bool wantDiff = true;

            if (wantDiff)
            {
                Assert.Equal(expectedCode.SanitizeCode(), code.SanitizeCode());
            }
            else
            {
                code.SanitizeCode().Should().Be(expectedCode.SanitizeCode());
            }
        }
    }

    public static void ShouldHaveUmlBehaviors(this Vertex vertex, string expected)
    {
        expected = SortStringByLines(expected);
        var actual = vertex.Behaviors.UmlDescription();
        actual = SortStringByLines(actual);

        // Assert.Equal(expected.ConvertLineEndingsToN(), vertex.Behaviors.UmlDescription());
        actual.ShouldBeShowDiff(expected);
    }

    private static List<string> SplitByLinesAndSort(string inputLines)
    {
        var list = inputLines.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        list.Sort();
        return list;
    }

    private static string SortStringByLines(string inputLines)
    {
        var lines = SplitByLinesAndSort(inputLines);
        var result = String.Join("\n", lines);

        return result;
    }

    public static void ShouldHaveChildrenAndUmlBehaviors(this Vertex vertex, string expectedChildren, string expectedBehaviors)
    {
        var expected = expectedChildren.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        var actual = new List<string>();

        foreach (var item in vertex.Children)
        {
            actual.Add(Vertex.Describe(item));
        }

        expected.Sort();
        actual.Sort();
        Assert.Equal(expected, actual);
        vertex.ShouldHaveUmlBehaviors(expectedBehaviors);
    }

    public static void ShouldBeShowDiff(this string actual, string expected, bool outputCleanActual = false, bool convertLineEndings = true)
    {
        if (convertLineEndings)
        {
            expected = expected.ConvertLineEndingsToN();
            actual = actual.ConvertLineEndingsToN();
        }

        if (expected != actual)
        {
            var diff = StringDiffer.Diff(expected, actual);

            if (outputCleanActual)
            {
                diff += "\n\n\nActual:\n" + actual;
            }

            Assert.True(false, diff);
        }
    }
}
