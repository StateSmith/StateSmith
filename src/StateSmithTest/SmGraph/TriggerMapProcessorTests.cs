#nullable enable

using FluentAssertions;
using StateSmith.Common;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmith.SmGraph.TriggerMap;
using System;
using System.Collections.Generic;
using Xunit;

namespace StateSmithTest.SmGraph.TriggerMap;

// https://github.com/StateSmith/StateSmith/issues/161
public class TriggerMapProcessorTests
{
    Dictionary<string, TriggerMapRule> matches = new();

    [Fact]
    public void SmallIntegrationTest()
    {
        var triggers = new List<string>{ "UP_PRESS", "UP_HELD", "OP1" };

        var input = """
            UPx => UP_PRESS, UP_HELD
            INPUT => UPx, OP1
            ANY => *
            """;

        var mapper = new TriggerMapper();
        mapper.Setup(input, triggers);
        mapper.MapTriggers("ANY".Split()).Should().BeEquivalentTo("UP_PRESS", "UP_HELD", "OP1");
        mapper.MapTriggers("UPx".Split()).Should().BeEquivalentTo("UP_PRESS", "UP_HELD");
    }

    [Fact]
    public void IntegrationTest1()
    {
        InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();
        inputSmBuilder.ConvertDiagramFileToSmVertices(TestHelper.GetThisDir() + "/" + "TriggerMap1.drawio");
        inputSmBuilder.FindSingleStateMachine();
        inputSmBuilder.FinishRunning();
        var sm = inputSmBuilder.GetStateMachine();
        NamedVertexMap map = new(sm);

        sm.ShouldHaveUmlBehaviors("""
            (INCREASE, DIM1, DIM2) / { log("any event"); }
            (enter, INCREASE, DIM1, DIM2, exit) / { log("any trigger"); }
            (enter, exit, INCREASE, DIM1, DIM2) / { log("$any_t"); }
            """);

        map.GetState("OFF").ShouldHaveUmlBehaviors("""
            (enter, DIM1, DIM2) / { light_off(); }
            INCREASE TransitionTo(ON1)
            """);
    }

    [Fact]
    public void IntegrationTest2()
    {
        InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();
        inputSmBuilder.ConvertDiagramFileToSmVertices(TestHelper.GetThisDir() + "/" + "TriggerMap2.drawio");
        inputSmBuilder.FindSingleStateMachine();
        inputSmBuilder.FinishRunning();
        var sm = inputSmBuilder.GetStateMachine();
        NamedVertexMap map = new(sm);

        sm.ShouldHaveUmlBehaviors("""
            (INC_ERR, ERR, INC1, INC2, DIM1, DIM2) / { log("unhandled event"); }
            INC_ERR / { err++; }
            """);

        map.GetState("OFF").ShouldHaveUmlBehaviors("""
            (DIM1, DIM2) / { buzz(); }
            INC1 TransitionTo(ON1)
            INC2 TransitionTo(ON2)
            """);

        map.GetState("ON1").ShouldHaveUmlBehaviors("""
            (DIM1, DIM2) TransitionTo(OFF)
            (INC1, INC2) TransitionTo(ON2)
            """);

        map.GetState("ERR").ShouldHaveUmlBehaviors("""
            (DIM1, INC1, DIM2, INC2) / { buzz(); }
            """);
    }

    [Fact]
    public void Simple()
    {
        // UP_PRESS, UP_HELD, OP1
        var input = """
            UPx => UP_PRESS, UP_HELD
            INPUT => UPx, OP1
            """;

        var matches = TriggerMapParser.MatchLines(input);

        TriggerMapper.PrepareMappings(matches, new List<string>());
        matches["UPx"].triggerMatchers.Should().BeEquivalentTo("UP_PRESS, UP_HELD".Split(", "));
        matches["INPUT"].triggerMatchers.Should().BeEquivalentTo("UP_PRESS, UP_HELD, OP1".Split(", "));
    }

    [Fact]
    public void SimpleReversed()
    {
        // UP_PRESS, UP_HELD, OP1
        var input = """
            UPx => UP_PRESS, UP_HELD
            INPUT => UPx, OP1
            """;

        ExpectParseRules(input, expectedCount: 2);
        ExpectParseRuleMatchers("UPx", "UP_PRESS, UP_HELD");
    }

    [Fact]
    public void ParseTest1()
    {
        var input = """
            UPx => UP_PRESS, UP_HELD
            DOWNx => DOWN_PRESS, DOWN_HELD
            x_PRESS => *_PRESS
            OPx => /OP\d+/, OP
            INPUT => UPx, DOWNx, OPx, ESC, OK // comment

            /* another comment */
            ANY => *
            """;
        ExpectParseRules(input, expectedCount: 6);
        ExpectParseRuleMatchers("UPx", "UP_PRESS, UP_HELD");
        ExpectParseRuleMatchers("DOWNx", "DOWN_PRESS, DOWN_HELD");
        ExpectParseRuleMatchers("x_PRESS", "*_PRESS");
        ExpectParseRuleMatchers("OPx", "/OP\\d+/, OP");
        ExpectParseRuleMatchers("INPUT", "UPx, DOWNx, OPx, ESC, OK");
        ExpectParseRuleMatchers("ANY", "*");
    }

    private void ExpectParseRules(string input, int expectedCount)
    {
        matches = TriggerMapParser.MatchLines(input);
        matches.Count.Should().Be(expectedCount);
    }

    private void ExpectParseRuleMatchers(string name, string expected)
    {
        name = TriggerHelper.SanitizeTriggerName(name);
        matches[name].triggerMatchers.Should().BeEquivalentTo(expected.Split(", "));
    }

    [Fact]
    public void ParseTest2()
    {
        var input = """UPx => UP_PRESS""";
        ExpectParseRules(input, expectedCount: 1);
        ExpectParseRuleMatchers("UPx", "UP_PRESS");
    }

    [Fact]
    public void ParseTestEnterShorthand()
    {
        var input = """en => enter""";
        ExpectParseRules(input, expectedCount: 1);
        ExpectParseRuleMatchers("en", "enter");
    }

    [Fact]
    public void ParseTestVariations()
    {
        void Test(string input)
        {
            ExpectParseRules(input, expectedCount: 1);
            ExpectParseRuleMatchers("UPx", "UP_PRESS, UP_HELD");
        }

        Test("""
            UPx => UP_PRESS,
                   UP_HELD,
            """);

        Test("""
            UPx => UP_PRESS
                   UP_HELD
            """);

        Test("""
            UPx => UP_PRESS
            UP_HELD
            """);

        Test("""
            UPx => UP_PRESS UP_HELD 
            """);

        Test("""
            UPx => UP_PRESS, UP_HELD 
            """);

        Test("""
            UPx => UP_PRESS,UP_HELD,
            """);
    }

    [Fact]
    public void ParseFail1()
    {
        var input = """
            DOWNx => DOWN_PRESS?, DOWN_HELD
            """;

        ExpectParseFailWithMessage(input, "*Trigger map*failed parsing rule 1*Invalid input: `DOWNx => DOWN_PRESS?,*");
    }

    [Fact]
    public void ParseFail2()
    {
        var input = """
            en => enter
            DOWNx => DOWN_PRESS?, DOWN_HELD
            """;

        ExpectParseFailWithMessage(input, "*Trigger map*failed parsing rule 2*Invalid input: `DOWNx => DOWN_PRESS?,*");
    }

    [Fact]
    public void ParseFailBadRegex()
    {
        var input = """
            en => enter
            DOWNx => /DOWN_PRESS
            """;

        ExpectParseFailWithMessage(input, "*Trigger map*failed parsing rule 2*Invalid input: `DOWNx => /DOWN_PRESS*");
    }

    [Fact]
    public void ThrowOnReservedEnter()
    {
        var input = """
            ev1 => some_ev2
            enter => bad_stuff
            """;
        ExpectParseFailWithMessage(input, "Trigger map failed parsing rule 2*Rule name `enter` can't match reserved* enter *");
    }

    [Fact]
    public void ThrowOnReservedExit()
    {
        var input = """
            ev1 => some_ev2
            exit => bad_stuff
            """;
        ExpectParseFailWithMessage(input, "Trigger map failed parsing rule 2*Rule name `exit` can't match reserved* exit *");
    }

    [Fact]
    public void ThrowOnReservedDo()
    {
        var input = """
            ev1 => some_ev2
            do => bad_stuff
            """;
        ExpectParseFailWithMessage(input, "Trigger map failed parsing rule 2*Rule name `do` can't match reserved `do` event*");
    }

    [Fact]
    public void TestRegexExpanding()
    {
        var eventList = "UP_PRESS, UP_HELD, OP1, OP2, OP3, OP10, ESC, OK".Split(", ");

        var input = """
            OPx => /OP\d/ // single digit
            UPx => UP_PRESS, UP_HELD
            INPUT => UPx, OPx
            ANY => *
            """;

        matches = TriggerMapParser.MatchLines(input);
        TriggerMapper.PrepareMappings(matches, eventList);

        ExpectParseRuleMatchers("OPx", "OP1, OP2, OP3");
        ExpectParseRuleMatchers("UPx", "UP_PRESS, UP_HELD");
        ExpectParseRuleMatchers("INPUT", "UP_PRESS, UP_HELD, OP1, OP2, OP3");
        ExpectParseRuleMatchers("ANY", "UP_PRESS, UP_HELD, OP1, OP2, OP3, OP10, ESC, OK");
    }

    [Fact]
    public void TestRegexExpanding_AnyTrigger()
    {
        var eventList = "do, do_stuff, UP_PRESS, UP_HELD".Split(", ");

        var input = """
            UPx => UP_*                    // UP_PRESS, UP_HELD
            ANY_EVENT => *                 // any event (includes 'do')
            ANY_TRIGGER => *, enter, exit  // any trigger (includes 'do')
            ANY_REGULAR => /...+/          // any regular event (excludes 'do' because it is only 2 chars long)
            ANY_REGULAR2 => /(?!do$).*/    // any regular event (excludes 'do' because of regex negative lookahead)
            """;

        matches = TriggerMapParser.MatchLines(input);
        TriggerMapper.PrepareMappings(matches, eventList);

        ExpectParseRuleMatchers("UPx", "UP_PRESS, UP_HELD");
        ExpectParseRuleMatchers("ANY_EVENT", "do, do_stuff, UP_PRESS, UP_HELD");
        ExpectParseRuleMatchers("ANY_TRIGGER", "enter, exit, do, do_stuff, UP_PRESS, UP_HELD");
        ExpectParseRuleMatchers("ANY_REGULAR", "do_stuff, UP_PRESS, UP_HELD");
        ExpectParseRuleMatchers("ANY_REGULAR2", "do_stuff, UP_PRESS, UP_HELD");
    }

    private static void ExpectParseFailWithMessage(string input, string ExpectedWildcardPattern)
    {
        Action a = () => TriggerMapParser.MatchLines(input);
        a.Should().Throw<ArgumentException>().WithMessage(ExpectedWildcardPattern);
    }
}
