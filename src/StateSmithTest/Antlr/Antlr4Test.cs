using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using StateSmith.Input.Antlr4;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;

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
        //Console.SetOut(new ConsoleCaptureConverter(output));

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
    }


    [Fact]
    public void MultipleBehaviors()
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

        textState.Behaviors[1].order.Should().Be(null);
        textState.Behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
        textState.Behaviors[1].guardCode.Should().Be(null);
        textState.Behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
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
        string input = StringUtils.ConvertToSlashNLines(@"
                OVEN_OFF
                event / { var += 3;
                    if (func(123))
                        stuff( func(8 * 2) );
                    if (true) {
                        a = ""hello there"";
                    }
                }
            ");
        var textState = (StateNode)ParseNodeWithNoErrors(input);
        textState.stateName.Should().Be("OVEN_OFF");
        textState.Behaviors.Count.Should().Be(1);
        textState.Behaviors[0].order.Should().Be(null);
        textState.Behaviors[0].triggers.Count.Should().Be(1);
        textState.Behaviors[0].guardCode.Should().Be(null);
        textState.Behaviors[0].actionCode.Should().Be("var += 3;\n" +
                                                      "if (func(123))\n" +
                                                      "    stuff( func(8 * 2) );\n" +
                                                      "if (true) {\n" +
                                                      "    a = \"hello there\";\n" +
                                                      "}"
                                                      );
    }

    [Fact]
    public void EdgeDeIndentMultilineAction()
    {
        string input = StringUtils.ConvertToSlashNLines(@"
                event / { var += 3;
                    if (func(123))
                        stuff( func(8 * 2) );
                    if (true) {
                        a = ""hello there"";
                    }
                }
            ");

        var behaviors = parser.ParseEdgeLabel(input);
        AssertNoErrors();
        behaviors.Count.Should().Be(1);
        behaviors[0].order.Should().Be(null);
        behaviors[0].triggers.Count.Should().Be(1);
        behaviors[0].guardCode.Should().Be(null);
        behaviors[0].actionCode.Should().Be("var += 3;\n" +
                                            "if (func(123))\n" +
                                            "    stuff( func(8 * 2) );\n" +
                                            "if (true) {\n" +
                                            "    a = \"hello there\";\n" +
                                            "}"
                                            );
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
        InputSmBuilder inputSmBuilder = new();
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
}
