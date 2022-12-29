using StateSmith.output;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System;
using Xunit;
using Xunit.Abstractions;
using StateSmithTest;

namespace Spec;

public class SpecTester
{
    public string PreEvents = "";

    protected List<string> events = new();
    protected List<Action<Action<string>>> testFunctions = new();
    int curIndex = 0;
    string actualChunk;

    public string EventList => string.Join(" ", events);
    public string PreAndNormalEvents => string.Join(" ", PreEvents, EventList);

    public bool HasExpectations()
    {
        return testFunctions.Any();
    }

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
            var diff = StringDiffer.Diff(expected, actualChunk);
            Assert.True(false, diff);
        }

        Assert.Equal(expected, actualChunk); // just in case
    }

    

    public void RunAndVerify(Func<string, string> processRunningFunction)
    {
        var output = processRunningFunction.Invoke(PreAndNormalEvents);
        Verify(output);
    }

    private void Reset()
    {
        this.events.Clear();
        this.testFunctions.Clear();
        this.curIndex = 0;
        this.actualChunk = "";
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
        Reset();
    }

    public static string PrepExpectedOutput(string expected)
    {
        expected = StringUtils.RemoveAnyIndentAndTrim(expected);
        expected = StringUtils.ReplaceNewLineChars(expected, "\n");
        return expected;
    }
}

