using StateSmith.Output;
using System.Collections.Generic;
using System.Linq;
using System;
using Xunit;
using StateSmithTest;

namespace Spec;

public class SpecTester
{
    public string PreEvents = "";

    protected List<string> events = new();
    protected List<Action<Action<string>>> testFunctions = new();
    int curIndex = 0;
    string actualChunk;
    public string Output => output;
    protected string output = "<none yet>";

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

            diff += "\n<expected>\n" + expected + "\n</expected>\n";
            diff += "\n<actual>\n" + actualChunk + "\n</actual>\n";
            Assert.True(false, diff);
        }

        Assert.Equal(expected, actualChunk); // just in case
    }

    public void RunAndVerify(Func<string, string> processRunningFunction)
    {
        output = processRunningFunction.Invoke(PreAndNormalEvents);
        Verify(output);
    }

    private void Reset()
    {
        events.Clear();
        testFunctions.Clear();
        curIndex = 0;
        actualChunk = "";
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

