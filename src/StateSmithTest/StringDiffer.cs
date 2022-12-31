using System;

namespace StateSmithTest;

public class StringDiffer
{
    public static string Diff(string expectedInput, string actualInput)
    {
        string diff = "";
        var expectedLines = expectedInput.Split("\n");
        var actualLines = actualInput.Split("\n");

        int j;

        for (j = 0; j < Math.Min(actualLines.Length, expectedLines.Length); j++)
        {
            var actualLine = actualLines[j];
            var expectedLine = expectedLines[j];

            if (expectedLine == actualLine)
            {
                diff += $" {expectedLine}\n";
            }
            else
            {
                diff += CreateDiffLocation(expectedLine, actualLine, '↓') + "\n";
                diff += $"-{expectedLine}\n";
                diff += $"+{actualLine}\n";
                diff += CreateDiffLocation(expectedLine, actualLine, '↑') + "\n";
            }
        }

        for (; j < Math.Max(actualLines.Length, expectedLines.Length); j++)
        {
            if (j < expectedLines.Length)
            {
                diff += $"-{expectedLines[j]}\n";
            }

            if (j < actualLines.Length)
            {
                diff += $"+{actualLines[j]}\n";
            }
        }

        return diff;
    }

    public static string CreateDiffLocation(string expected, string actual, object indicatorChar)
    {
        return new string('·', FindDiffLocation(expected, actual) + 1) + indicatorChar;
    }

    public static int FindDiffLocation(string expected, string actual)
    {
        int i;

        for (i = 0; i < expected.Length && i < actual.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                return i;
            }
        }

        return i;
    }
}