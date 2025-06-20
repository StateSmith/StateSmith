using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using StateSmith.Input.Antlr4;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;

// todolow look into this: https://www.antlr.org/api/Java/org/antlr/v4/runtime/TokenStreamRewriter.html

namespace StateSmithTest.Antlr;

public class Antlr4Test : CommonTestHelper
{
    public Antlr4Test(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void StateNameOnly()
    {
        string input = @"
                SOME_SM_STATE_NAME
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("SOME_SM_STATE_NAME");
        textState.Behaviors.Count.Should().Be(0);
    }

    [Fact]
    public void Test1()
    {
        string input = @"
                SOME_SM_STATE_NAME
                11. MY_EVENT [some_guard( ""my }str with spaces"" ) && blah] / my_action();
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("SOME_SM_STATE_NAME");
        textState.Behaviors.Count.Should().Be(1);
        textState.Behaviors[0].order.Should().Be("11");
        textState.Behaviors[0].triggers.Should().BeEquivalentTo(new string[] { "MY_EVENT" });
        textState.Behaviors[0].guardCode.Should().Be(@"some_guard( ""my }str with spaces"" ) && blah");
        textState.Behaviors[0].actionCode.Should().Be("my_action();");

        ShouldBeEquivalentToUml(textState.Behaviors[0], """11. MY_EVENT [some_guard( "my }str with spaces" ) && blah] / { my_action(); }""");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/230
    /// </summary>
    [Fact]
    public void AllowSlashInGuardOrActionCode_230()
    {
        string input = @"
                SOME_SM_STATE_NAME
                [a > c / 4] / do_stuff(a/4);
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("SOME_SM_STATE_NAME");
        textState.Behaviors.Count.Should().Be(1);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Should().BeEquivalentTo([]);
        textState.Behaviors[0].guardCode.Should().Be(@"a > c / 4");
        textState.Behaviors[0].actionCode.Should().Be("do_stuff(a/4);");

        ShouldBeEquivalentToUml(textState.Behaviors[0], """[a > c / 4] / { do_stuff(a/4); }""");
    }

    [Fact]
    public void AllowSquareBracesInGuardOrActionCode()
    {
        string input = @"
                SOME_SM_STATE_NAME
                [arr[33] > 2] / do_stuff(arr[22]);
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("SOME_SM_STATE_NAME");
        textState.Behaviors.Count.Should().Be(1);
        ShouldBeEquivalentToUml(textState.Behaviors[0], """[arr[33] > 2] / { do_stuff(arr[22]); }""");
    }

    [Fact]
    public void NodeMultipleBehaviors()
    {
        string input = @"
                a_lowercase_state_name
                [ true ] / { }
                event / { action_code(123); }
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("a_lowercase_state_name");
        textState.Behaviors.Count.Should().Be(2);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Count.Should().Be(0);
        textState.Behaviors[0].guardCode.Should().Be("true");
        textState.Behaviors[0].actionCode.Trim().Should().Be("");
        ShouldBeEquivalentToUml(textState.Behaviors[0], """[true]""");

        textState.Behaviors[1].order.Should().Be(null);
        textState.Behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
        textState.Behaviors[1].guardCode.Should().Be(null);
        textState.Behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
        ShouldBeEquivalentToUml(textState.Behaviors[1], """event / { action_code(123); }""");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/277
    /// </summary>
    [Fact]
    public void ExitUsedInActionCode_277()
    {
        string input = @"
                MY_STATE
                event1 / exit();
                event1 / exit = 2;
                event1 / system.exit();
                event1 / EXIT();
                event1 / EXIT = 22;
                event1 / system.EXIT();
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        int i = 0;
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { exit(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { exit = 2; }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { system.exit(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { EXIT(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { EXIT = 22; }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { system.EXIT(); }""");
        textState.Behaviors.Count.Should().Be(i);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/277
    /// </summary>
    [Fact]
    public void EntryUsedInActionCode_277()
    {
        string input = @"
                MY_STATE
                event1 / entry();
                event1 / entry = 2;
                event1 / system.entry();
                event1 / ENTRY();
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        int i = 0;
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { entry(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { entry = 2; }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { system.entry(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { ENTRY(); }""");
        textState.Behaviors.Count.Should().Be(i);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/211
    /// </summary>
    [Fact]
    public void ViaUsedInActionCode_211()
    {
        string input = @"
                MY_STATE
                event1 / via();
                event1 / via = 2;
                event1 / system.via();
                event1 / VIA();
                event1 / VIA = 22;
                event1 / system.VIA();
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        int i = 0;
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { via(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { via = 2; }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { system.via(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { VIA(); }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { VIA = 22; }""");
        ShouldBeEquivalentToUml(textState.Behaviors[i++], """event1 / { system.VIA(); }""");
        textState.Behaviors.Count.Should().Be(i);
    }

    [Fact]
    public void NodeMultipleBehaviors2()
    {
        string input = @"
                a_lowercase_state_name
                event1
                [ true ] / { }
                event / { action_code(123); }
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.Behaviors.Count.Should().Be(3);
        ShouldBeEquivalentToUml(textState.Behaviors[0], """event1""");
        ShouldBeEquivalentToUml(textState.Behaviors[1], """[true]""");
        ShouldBeEquivalentToUml(textState.Behaviors[2], """event / { action_code(123); }""");
    }

    /// <summary>
    /// Relates to https://github.com/StateSmith/StateSmith/issues/164
    /// </summary>
    [Fact]
    public void EntryExitTrailingNewlines()
    {
        string input = """
            event1/
            via exit done
            via entry skip_start
            """;
        var behaviors = ParseEdgeWithoutErrors(input);
        behaviors.Count.Should().Be(1);
        ShouldBeEquivalentToUml(behaviors[0], "event1 via exit done via entry skip_start");
    }

    /// <summary>
    /// Relates to https://github.com/StateSmith/StateSmith/issues/164
    /// </summary>
    [Fact]
    public void ExitEntryBeforeBehaviors()
    {
        string input = """
            via exit done
            via entry skip_start
            event1/
            """;
        var behaviors = ParseEdgeWithoutErrors(input);
        behaviors.Count.Should().Be(2);
        ShouldBeEquivalentToUml(behaviors[0], "via exit done via entry skip_start");
        ShouldBeEquivalentToUml(behaviors[1], "event1");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/43
    /// </summary>
    [Fact]
    public void ShortHandForCatchingEvents_43()
    {
        string input = @"
                a_lowercase_state_name
                event1/";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.Behaviors.Count.Should().Be(1);

        ParseEdgeWithoutErrors("event/").Count.Should().Be(1);
    }

    [Fact]
    public void MultilineAction()
    {
        string input = @"
                STATE123
                event / { 
                  action_code(123);
                }
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("STATE123");
        textState.Behaviors.Count.Should().Be(1);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Count.Should().Be(1);
        textState.Behaviors[0].guardCode.Should().Be(null);
        textState.Behaviors[0].actionCode.Trim().Should().Be("action_code(123);");
    }

    [Fact]
    public void DeIndentMultilineAction()
    {
        string input = StringUtils.ConvertToSlashNLines("""
            OVEN_OFF
            event / { var += 3;
                if (func(123))
                    stuff( func(8 * 2) );
                if (true) {
                    a = "hello there";
                }
            }
            """);
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("OVEN_OFF");
        textState.Behaviors.Count.Should().Be(1);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Count.Should().Be(1);
        textState.Behaviors[0].guardCode.Should().Be(null);
        textState.Behaviors[0].actionCode.Should().Be("""
            var += 3;
            if (func(123))
                stuff( func(8 * 2) );
            if (true) {
                a = "hello there";
            }
            """
        );
    }

    [Fact]
    public void EdgeDeIndentMultilineAction()
    {
        string input = StringUtils.ConvertToSlashNLines("""
            event / { var += 3;
                if (func(123))
                    stuff( func(8 * 2) );
                if (true) {
                    a = "hello there";
                }
            }
            """);

        var behaviors = parser.ParseEdgeLabel(input);
        AssertNoErrors();
        behaviors.Count.Should().Be(1);
        behaviors[0].order.Should().Be(null);
        behaviors[0].triggers.Count.Should().Be(1);
        behaviors[0].guardCode.Should().Be(null);
        behaviors[0].actionCode.Should().Be("""
            var += 3;
            if (func(123))
                stuff( func(8 * 2) );
            if (true) {
                a = "hello there";
            }
            """
        );
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/478
    /// </summary>
    [Fact]
    public void PreprocessorConditionalActionCode_478()
    {
        // test #ifdef
        TestParsingSingleEdgeLabelBehavior(
            input: 
                """
                event / {
                    #ifdef ENABLE_SOME_FEATURE
                    do_stuff();
                    #endif
                }
                """,
            expectedSingleLineUml: 
                """
                event / { #ifdef ENABLE_SOME_FEATURE\ndo_stuff();\n#endif }
                """
        );

        // test #if
        TestParsingSingleEdgeLabelBehavior(
            input:
                """
                event / {
                    #if ENABLE_SOME_FEATURE
                    do_stuff();
                    #else
                    do_other_stuff();
                    #endif
                }
                """,
            expectedSingleLineUml: 
                """
                event / { #if ENABLE_SOME_FEATURE\ndo_stuff();\n#else\ndo_other_stuff();\n#endif }
                """
        );
    }

    /// <summary>
    /// This test just shows that these keywords can be parsed correctly in action code.
    /// We DO NOT specifically allow or deny them at this point. Avoid using them in action code
    /// if you can.
    /// </summary>
    [Fact]
    public void KeywordsInActionCode()
    {
        string[] keywords = ["$NOTES", "$CONFIG", "$ORTHO", "$choice", "$STATEMACHINE"];

        foreach (string keyword in keywords)
        {
            TestParsingSingleEdgeLabelBehavior(
                input: $$"""my_event / { {{keyword}}++; }""",
                expectedSingleLineUml: $$"""my_event / { {{keyword}}++; }"""
            );
        }
    }

    [Fact]
    public void OrthoMultipleBehaviors()
    {
        string input = @"
                $ORTHO : a_lowercase_state_name
                [ true ] / { }
                event / { action_code(123); }
            ";
        var textState = (OrthoStateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("a_lowercase_state_name");
        textState.order.Should().Be(null);
        textState.Behaviors.Count.Should().Be(2);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Count.Should().Be(0);
        textState.Behaviors[0].guardCode.Should().Be("true");
        textState.Behaviors[0].actionCode.Trim().Should().Be("");

        textState.Behaviors[1].order.Should().Be(null);
        textState.Behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
        textState.Behaviors[1].guardCode.Should().Be(null);
        textState.Behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
    }

    [Fact]
    public void OrthoOrderMultipleBehaviors()
    {
        string input = @"
                $ORTHO  26.7 : a_lowercase_state_name
                [ true ] / { }
                event / { action_code(123); }
            ";
        var textState = (OrthoStateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("a_lowercase_state_name");
        textState.order.Should().Be("26.7");
        textState.Behaviors.Count.Should().Be(2);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Count.Should().Be(0);
        textState.Behaviors[0].guardCode.Should().Be("true");
        textState.Behaviors[0].actionCode.Trim().Should().Be("");

        textState.Behaviors[1].order.Should().Be(null);
        textState.Behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
        textState.Behaviors[1].guardCode.Should().Be(null);
        textState.Behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
    }

    [Fact]
    public void StateMachineNode()
    {
        string input = @"
                $STATEMACHINE : MicrowaveSm
            ";
        var node = (StateMachineNode)ParseNodeWithNoErrors(input);
        node.name.Should().Be("MicrowaveSm");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/163
    /// </summary>
    [Fact]
    public void StateMachineNodeWithBehaviors()
    {
        string input = """
            $STATEMACHINE : MicrowaveSm
            enter / x++;
            2. DEC [x > 1] / y--;
            """;
        var sm = (StateMachineNode)ParseNodeWithNoErrors(input);
        sm.name.Should().Be("MicrowaveSm");
        sm.Behaviors.Count.Should().Be(2);
        //
        sm.Behaviors[0].order.Should().Be(null);
        sm.Behaviors[0].triggers.Should().BeEquivalentTo("enter");
        sm.Behaviors[0].guardCode.Should().BeNull();
        sm.Behaviors[0].actionCode.Trim().Should().Be("x++;");
        //
        sm.Behaviors[1].order.Should().Be("2");
        sm.Behaviors[1].triggers.Should().BeEquivalentTo("DEC");
        sm.Behaviors[1].guardCode.Should().Be("x > 1");
        sm.Behaviors[1].actionCode.Trim().Should().Be("y--;");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/163
    /// </summary>
    [Fact]
    public void StateMachineWithBehaviorsIntegrationTest()
    {
        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertDiagramFileToSmVertices(TestHelper.GetThisDir() + "/" + "Antlr4Tests.drawio");
        inputSmBuilder.FindStateMachineByName("MySm1");
        inputSmBuilder.FinishRunning();
        var sm = inputSmBuilder.GetStateMachine();

        sm.Name.Should().Be("MySm1");
        sm.ShouldHaveUmlBehaviors("""
            enter / { print("MySm1 enter"); }
            do / { print("MySm1 do"); }
            """);
    }

    [Fact]
    public void NotesNode()
    {
        string input = "$NOTES this is my note!!! /* Not an actual comment test\n" +
                        "Another line of 2134 \"notes\"\n";
        var node = (NotesNode)ParseNodeWithNoErrors(input);
        node.notes.Should().Be("this is my note!!! /* Not an actual comment test\n" +
                        "Another line of 2134 \"notes\"");
    }

    // https://github.com/StateSmith/StateSmith/issues/60
    [Fact]
    public void SingleLetter_e_Function()
    {
        string input = @"
            S2_1
            enter / e();
            ";
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("S2_1");
        textState.Behaviors.Count.Should().Be(1);
        textState.Behaviors[0].triggers.Should().BeEquivalentTo(new string[] { "enter" });
        textState.Behaviors[0].actionCode.Should().Be("e();");
    }
    
    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/42
    /// </summary>
    [Fact]
    public void NotesNodeWithBackticks()
    {
        string input = "$NOTES this is my `note`";
        var node = (NotesNode)ParseNodeWithNoErrors(input);
        node.notes.Should().Be("this is my `note`");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/42
    /// </summary>
    [Fact]
    public void NotesNodeWithSingleBacktick()
    {
        string input = "$NOTES this is my `note";
        var node = (NotesNode)ParseNodeWithNoErrors(input);
        node.notes.Should().Be("this is my `note");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/42
    /// </summary>
    [Fact]
    public void NotesNodeWithCharacters()
    {
        string input = "$NOTES ~!@#$%^&*()_+`-=[]{}\\|;:'\",<.>/?";
        var node = (NotesNode)ParseNodeWithNoErrors(input);
        node.notes.Should().Be("~!@#$%^&*()_+`-=[]{}\\|;:'\",<.>/?");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/42
    /// </summary>
    [Fact]
    public void NotesNodeWithUnbalancedCharacters()
    {
        var node = (NotesNode)ParseNodeWithNoErrors("$NOTES ([{<");
        node.notes.Should().Be("([{<");

        node = (NotesNode)ParseNodeWithNoErrors("$NOTES )]}>");
        node.notes.Should().Be(")]}>");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/42
    /// </summary>
    [Fact]
    public void NotesNodeWithAsciiChars()
    {
        var input = "a\r\t\n"; // start with non-whitespace character so that parser doesnt' strip it

        for (char c = ' '; c <= '~'; c++)
        {
            input += c;
        }

        var node = (NotesNode)ParseNodeWithNoErrors("$NOTES " + input);
        node.notes.Should().Be(input);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/455
    /// </summary>
    [Fact]
    public void NotesNodeWithNonAsciiChars_455()
    {
        var input = "ë, Ш";
        var node = (NotesNode)ParseNodeWithNoErrors("$NOTES " + input);
        node.notes.Should().Be(input);
    }

    /// <summary>
    /// Test workaround for
    /// https://github.com/StateSmith/StateSmith/issues/455
    /// </summary>
    [Fact]
    public void NotesNodeWithTripleQuotedNonAsciiChars_455()
    {
        // triple quoted work around
        {
            var input = """
            '''
            ë, Ш
            '''
            """;
            var node = (NotesNode)ParseNodeWithNoErrors("$NOTES " + input);
            node.notes.Should().Be(input);
        }

        // backtick workaround
        {
            var input = """
            `
            ë, Ш
            `
            """;
            var node = (NotesNode)ParseNodeWithNoErrors("$NOTES " + input);
            node.notes.Should().Be(input);
        }

        // comment workaround
        {
            var input = """
            /*
            ë, Ш
            */
            """;
            var node = (NotesNode)ParseNodeWithNoErrors("$NOTES " + input);
            node.notes.Should().Be(input);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////

    private void TestParsingSingleEdgeLabelBehavior(string input, string expectedSingleLineUml)
    {
        input = StringUtils.ConvertToSlashNLines(input);
        var behavior = parser.ParseEdgeLabel(input).Single();
        AssertNoErrors();
        ShouldBeEquivalentToUml(behavior, expectedSingleLineUml);
    }

    private static void ShouldBeEquivalentToUml(NodeBehavior nodeBehavior, string uml)
    {
        ConvertNodeBehaviorToBehavior(nodeBehavior).DescribeAsUml().Should().Be(uml);
    }

    public static Behavior ConvertNodeBehaviorToBehavior(NodeBehavior nodeBehavior)
    {
        return DiagramToSmConverter.ConvertBehavior(new State("dummy"), nodeBehavior);
    }
}
