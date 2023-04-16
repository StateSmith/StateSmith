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
        ExpectOk("100 millis ", 100);

        ExpectOk("1s", 1000);
        ExpectOk("1 second", 1000);
        ExpectOk("3 seconds", 3000);

        ExpectOk("4.25 min", (int)(4.25 * 60 * 1000));
        ExpectOk("6.50 minutes", (int)(6.5 * 60 * 1000));

        ExpectOk("8h", 8 * 60 * 60 * 1000);
        ExpectOk("1hour", 1 * 60 * 60 * 1000);
        ExpectOk("12hrs", 12 * 60 * 60 * 1000);
        ExpectOk("8 hours", 8 * 60 * 60 * 1000);

        ExpectOk("0.65 min", (int)(0.65 * 60 * 1000));
        ExpectOk("1 minute", (int)(1 * 60 * 1000));
        ExpectOk("15 minutes", (int)(15 * 60 * 1000));

        ExpectOk("52398 s", 52398 * 1000);
        ExpectOk("777 secs", 777 * 1000);
        ExpectOk("444 seconds", 444 * 1000);

    }

    [Fact]
    public void BadInput()
    {
        ExpectBadInput("+1.25 s");
        ExpectBadInput("5 long arse days");
        ExpectBadInput("7 average looking horses");
        ExpectBadInput("2 balls of ice cream");
        ExpectBadInput("19 birthday cakes");
        ExpectBadInput("0.9 successful relationships");
        ExpectBadInput("100 days of coding");
        ExpectBadInput("-15 blissful sniffs");
    }

    [Fact]
    public void BadTimeUnits()
    {
        ExpectBadTimeUnit("125 ns", badUnits: "ns");
        ExpectBadTimeUnit("125 ns", badUnits: "ns");
        ExpectBadTimeUnit("909 picoseconds", badUnits: "picoseconds");
        ExpectBadTimeUnit("3 businessDays", badUnits: "businessDays");
        ExpectBadTimeUnit("1 lunarYear", badUnits: "lunarYear");
        ExpectBadTimeUnit("3 longSighs", badUnits: "longSighs");
        ExpectBadTimeUnit("70 sneezes", badUnits: "sneezes");
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
