using StateSmith.output;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System;
using Xunit;
using Xunit.Abstractions;

/*
 * This file is intended to provide language agnostic helpers and expansions for all specification tests
 */

namespace Spec;

public class OutputExpectation2
{
    protected List<string> events = new();
    protected List<Func<string>> expectationBuilders = new();

    public void AddEventHandling(string eventName, Func<string> expectationBuilder)
    {
        events.Add(eventName);
        expectationBuilders.Add(expectationBuilder);
    }
}


public class OutputExpectation
{
    protected ITestOutputHelper xunitOutput;
    protected List<string> expectedChunks = new();
    protected List<string> events = new();
    protected List<Func<string>> expectationBuilders = new();


    public OutputExpectation(ITestOutputHelper xunitOutput)
    {
        this.xunitOutput = xunitOutput;
    }

    public void AddWithoutEvent(string chunk)
    {
        AddRaw(PrepExpectedOutput(chunk));
    }

    private void AddRaw(string chunk)
    {
        expectedChunks.Add(chunk);
    }

    public void AddEventHandling(string eventName, Func<string> expectationBuilder)
    {
        events.Add(eventName);
        expectationBuilders.Add(expectationBuilder);
    }

    public void AddEventHandling(string eventName, string chunk)
    {
        events.Add(eventName);

        var combined = PrepExpectedOutput($@"
            Dispatch event {eventName}
            ===================================================
        ") + "\n";
        combined += PrepExpectedOutput(chunk);

        AddRaw(combined);
    }

    public string EventList => string.Join(" ", events);

    public string Verify2(string expected)
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

        //actualChunk.Split("\n").Should().BeEquivalentTo(expected.Split("\n"));

        Assert.Equal(expected, actualChunk);
        return "";
    }

    public string CreateDiffDiff(string expectedChunk, string actualChunk)
    {
        string diff = "";
        var expectedLines = expectedChunk.Split("\n");
        var actualLines = actualChunk.Split("\n");

        for (int j = 0; j < Math.Min(actualLines.Length, expectedLines.Length); j++)
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

        return diff;
    }

    public string CreateDiffLocation(string expected, string actual, object indicatorChar)
    {
        return new string('·', FindDiffLocation(expected, actual) + 1) + indicatorChar;
    }

    public int FindDiffLocation(string expected, string actual)
    {
        int i = 0;

        for (i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                return i;
            }
        }

        return i;
    }

    int curIndex = 0;
    string actualChunk;

    public void Verify(string output)
    {
        var actualChunks = output.Split("\n\n");

        for (int i = 0; i < Math.Min(actualChunks.Length, expectationBuilders.Count); i++)
        {
            actualChunk = actualChunks[i];
            expectationBuilders[i].Invoke();

            curIndex++;
        }

        Assert.Equal(expectationBuilders.Count, actualChunks.Length);
    }

    public void VerifyOld(string output)
    {
        var actualChunks = output.Split("\n\n");

        //outputChunks.Should().BeEquivalentTo(expectedChunks);

        for (int i = 0; i < Math.Min(actualChunks.Length, expectedChunks.Count); i++)
        {
            string actualChunk = actualChunks[i];
            string expectedChunk = expectedChunks[i];

            var expectedLines = expectedChunk.Split("\n");
            var actualLines = actualChunk.Split("\n");

            for (int j = 0; j < Math.Min(actualLines.Length, expectedLines.Length); j++)
            {
                var actual = actualLines[j];
                var expected = expectedLines[j];

                if (expected != actual)
                {
                    xunitOutput.WriteLine($"Failed at line {j} (0 based)");
                }
                Assert.Equal(expected, actual);
            }

            //actual.Should().Be(expected);
            //Assert.Equal(expected, actual);
        }

        //int i = 0;

        //while (true)
        //{
        //    if (outputChunks)
        //    i++;
        //}
    }

    public static string PrepExpectedOutput(string expected)
    {
        expected = StringUtils.DeIndentTrim(expected);
        expected = StringUtils.ReplaceNewLineChars(expected, "\n");
        return expected;
    }
}

