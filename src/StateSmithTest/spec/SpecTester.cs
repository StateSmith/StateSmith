using StateSmith.output;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Spec;

public class SpecTester
{
    public string PreEvents = "";

    public Func<string, string, string> SpecificationRunner;

    protected List<string> events = new();
    protected List<Action<Action<string>>> testFunctions = new();
    int curIndex = 0;
    string actualChunk;


    public string EventList => string.Join(" ", events);

    public void AddEventHandling(string eventName, Action<Action<string>> testFunction)
    {
        events.Add(eventName);
        testFunctions.Add(testFunction);
    }

    internal void TestChunk(string expected)
    {
        var combined = PrepExpectedOutput($@"
            Dispatch event {events[curIndex]}
            ===================================================
        ") + "\n";
        combined += PrepExpectedOutput(expected);

        expected = combined;

        if (expected != actualChunk)
        {
            var diff = CreateDiffDiff(expected, actualChunk);
            Assert.True(false, diff);
        }

        Assert.Equal(expected, actualChunk); // just in case
    }

    public static string CreateDiffDiff(string expectedChunk, string actualChunk)
    {
        string diff = "";
        var expectedLines = expectedChunk.Split("\n");
        var actualLines = actualChunk.Split("\n");

        int j;

        for (j = 0; j < Math.Min(actualLines.Length, expectedLines.Length); j++)
        {
            var actual = actualLines[j];
            var expected = expectedLines[j];

            if (expected == actual)
            {
                diff += $" {expected}\n";
            }
            else
            {
                diff += CreateDiffLocation(expected, actual, '↓') + "\n";
                diff += $"-{expected}\n";
                diff += $"+{actual}\n";
                diff += CreateDiffLocation(expected, actual, '↑') + "\n";
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

    public void RunAndVerify()
    {
        var output = SpecificationRunner.Invoke(PreEvents, EventList);
        Verify(output);
    }

    public void Verify(string output)
    {
        var actualChunks = output.Split("\n\n");

        for (int i = 0; i < Math.Min(actualChunks.Length, testFunctions.Count); i++)
        {
            actualChunk = actualChunks[i];
            testFunctions[i].Invoke(TestChunk);

            curIndex++;
        }

        Assert.Equal(testFunctions.Count, actualChunks.Length);
    }

    public static string PrepExpectedOutput(string expected)
    {
        expected = StringUtils.DeIndentTrim(expected);
        expected = StringUtils.ReplaceNewLineChars(expected, "\n");
        return expected;
    }
}

