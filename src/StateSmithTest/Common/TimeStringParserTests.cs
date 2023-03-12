using Antlr4.Runtime.Misc;
using FluentAssertions;
using StateSmith.Common;
using System;
using Xunit;

namespace StateSmithTest.Common;

public class TimeStringParserTests
{
    [Fact]
    public void ValidInput()
    {
        ExpectOk("77 ms", 77);
        ExpectOk(" 77 milliseconds ", 77);

        ExpectOk("1s", 1000);
        ExpectOk("1 s", 1000);

        ExpectOk("4.25 min", (int)(4.25 * 60 * 1000));

        ExpectOk("8h", 8 * 60 * 60 * 1000);
        ExpectOk("8 hours", 8 * 60 * 60 * 1000);

        // TODO add all variations https://github.com/StateSmith/StateSmith/issues/131
    }

    [Fact]
    public void BadInput()
    {
        ExpectBadInput("+1.25 s");
        ExpectBadInput("5 long arse days");

        // TODO add more https://github.com/StateSmith/StateSmith/issues/131
    }

    [Fact]
    public void BadTimeUnits()
    {
        ExpectBadTimeUnit("125 ns", badUnits: "ns");

        // TODO add more https://github.com/StateSmith/StateSmith/issues/131
    }

    private static void ExpectOk(string timeString, int expectatedMs)
    {
        TimeStringParser.ElapsedTimeStringToMs(timeString).Should().Be(expectatedMs);
    }

    private static void ExpectBadInput(string timeString)
    {
        Action action = () => TimeStringParser.ElapsedTimeStringToMs(timeString);
        action.Should().Throw<ArgumentException>().WithMessage("Failed to match elapsed time for input: " + timeString);
    }

    private static void ExpectBadTimeUnit(string timeString, string badUnits)
    {
        Action action = () => TimeStringParser.ElapsedTimeStringToMs(timeString);
        action.Should().Throw<ArgumentException>().WithMessage("Unsupported time units: " + badUnits);
    }
}
