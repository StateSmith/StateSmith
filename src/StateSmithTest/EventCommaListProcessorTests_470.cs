using StateSmith.Output.UserConfig;
using StateSmith.SmGraph;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System;

namespace StateSmithTest;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/470
/// </summary>
public class EventCommaListProcessorTests_470
{
    [Fact]
    public void TestElementParsing_Valid()
    {
        TestParseElement(name: "ev1", id: "", input: "ev1");

        TestParseElement(name: "ev1", id: "22", input: "ev1 = 22");
        TestParseElement(name: "ev1", id: "22", input: "ev1 => 22");

        TestParseElement(name: "ev1", id: "SYSTEM_EV1", input: "ev1 = SYSTEM_EV1");
        TestParseElement(name: "ev1", id: "SYSTEM_EV1", input: "ev1 => SYSTEM_EV1");

        TestParseElement(name: "someEVENT", id: "SYSTEM_EV1", input: "someEVENT=SYSTEM_EV1");
        TestParseElement(name: "someEVENT", id: "SYSTEM_EV1", input: "someEVENT=>SYSTEM_EV1");
    }

    [Fact]
    public void TestElementParsing_Invalid()
    {
        ExpectParseElementFailure("ev1=22;");

        ExpectParseElementFailure("ev1=");
        ExpectParseElementFailure("ev1=>");
        ExpectParseElementFailure("ev1 =");
        ExpectParseElementFailure("ev1 =>");
        ExpectParseElementFailure("ev1 = ");
        ExpectParseElementFailure("ev1 => ");
        ExpectParseElementFailure("=1");
        ExpectParseElementFailure("=>1");
        ExpectParseElementFailure(" = 1");
        ExpectParseElementFailure(" => 1");
        ExpectParseElementFailure(" = 1 ");
        ExpectParseElementFailure(" => 1 ");
        ExpectParseElementFailure("ev1 : 1");
    }

    [Fact]
    public void TestMapping_Valid()
    {
        AssertEventMapping("ev1, ev2, ev3", new Dictionary<string, string>
        {
            { "ev1", "" },
            { "ev2", "" },
            { "ev3", "" }
        });

        AssertEventMapping("ev1 = 33, ev2=>FUN,", new Dictionary<string, string>
        {
            { "ev1", "33" },
            { "ev2", "FUN" },
        });

        // allow \r as separator
        AssertEventMapping("ev1 = 33\r ev2=>FUN,", new Dictionary<string, string>
        {
            { "ev1", "33" },
            { "ev2", "FUN" },
        });
        AssertEventMapping("ev1\rev2,", new Dictionary<string, string>
        {
            { "ev1", "" },
            { "ev2", "" },
        });

        // allow \n as separator
        AssertEventMapping("ev1 = 33\n ev2=>FUN,", new Dictionary<string, string>
        {
            { "ev1", "33" },
            { "ev2", "FUN" },
        });

        // allow \r\n as separator
        AssertEventMapping("ev1 = 33\r\n ev2=>FUN,", new Dictionary<string, string>
        {
            { "ev1", "33" },
            { "ev2", "FUN" },
        });
    }

    [Fact]
    public void TestMapping_InvalidSyntax()
    {
        Action act;

        // missing comma separator     
        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "ev1 ev2");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed processing user RenderConfig.EventCommaList string: `ev1 ev2`*Mapping so far: `{}`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Failed parsing event mapping item*`ev1 ev2`*");

        // first parses successfully then missing comma separator
        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "ev1, ev2 ev3");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed*`ev1, ev2 ev3`*Mapping*`{"ev1":""}`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Failed parsing event mapping item*`ev2 ev3`*");

        // test error message with newlines
        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), """
            ev1 = 22
            ev2 => SYS2;
            """.ConvertLineEndingsToN());
        act.Should().Throw<ArgumentException>()
            .WithMessage("""
            Failed *`ev1 = 22
            ev2 => SYS2;`*Mapping*`{"ev1":"22"}`*
            """.ConvertLineEndingsToN())
            .WithInnerException<ArgumentException>()
            .WithMessage("Failed parsing event mapping item*`ev2 => SYS2;`*");
    }

    [Fact]
    public void TestMapping_Invalid_RepeatedEvent()
    {
        Action act;

        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "ev1, ev1");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed processing user RenderConfig.EventCommaList string: `ev1, ev1`*Mapping so far: `{"ev1":""}`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Event `ev1` has already been mapped. Each event can only be specified once.")
            ;
    }

    [Fact]
    public void TestMapping_Invalid_RepeatedValue()
    {
        Action act;

        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "ev1=1, ev2=1");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed*`ev1=1, ev2=1`*Mapping*`{"ev1":"1","ev2":"1"}`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Multiple events were mapped to value `1`. Not allowed.");
    }

    /// <summary>
    /// If any event value is specified, then all events must have a value.
    /// </summary>
    [Fact]
    public void TestMapping_Invalid_PartiallySpecified()
    {
        Action act;

        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "ev1=1, ev2");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed*`ev1=1, ev2`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Mapping for event `ev2` must specify a value because other events have a specified value.*");
    }

    [Fact]
    public void TestMapping_Invalid_ReservedNames()
    {
        Action act;

        // reserve ENTER
        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "enter, ev2");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed*`enter, ev2`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Invalid event mapping for `enter`. That is a reserved trigger and not an event.");

        // reserve EXIT
        act = () => EventCommaListProcessor.ParseStringToEventMapping(new(), "ev1, exit");
        act.Should().Throw<ArgumentException>()
            .WithMessage("""Failed*`ev1, exit`*""")
            .WithInnerException<ArgumentException>()
            .WithMessage("Invalid event mapping for `exit`.*");
    }

    private void TestParseElement(string name, string id, string input)
    {
        EventCommaListProcessor.ParseEventElement(input, out string eventName, out string eventId);
        eventName.Should().Be(name);
        eventId.Should().Be(id);
    }

    private void ExpectParseElementFailure(string input)
    {
        Action act = () => EventCommaListProcessor.ParseEventElement(input, out string eventName, out string eventId);
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Failed parsing event mapping item*`{input}`*");
    }

    private static void AssertEventMapping(string input, Dictionary<string, string> expected)
    {
        EventMapping mapping = new();
        EventCommaListProcessor.ParseStringToEventMapping(mapping, input);
        mapping.UnsanitizedMap.Should().BeEquivalentTo(expected);
    }
}
