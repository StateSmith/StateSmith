using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using Xunit.Abstractions;
using StateSmith.Input.antlr4;

//todolow look into this: https://www.antlr.org/api/Java/org/antlr/v4/runtime/TokenStreamRewriter.html

namespace StateSmithTest
{
    public class Antlr4Test
    {
        ITestOutputHelper output;
        LabelParser parser = new LabelParser();

        public Antlr4Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void AssertNoErrors()
        {
            parser.errorListener.errors.Count.Should().Be(0);
        }

        [Fact]
        public void StateNameOnly()
        {
            string input = @"
                SOME_SM_STATE_NAME
            ";
            var textState = (StateNode)ParseNodeWithNoErrors(input);
            textState.stateName.Should().Be("SOME_SM_STATE_NAME");
            textState.behaviors.Count.Should().Be(0);
        }

        private Node ParseNodeWithNoErrors(string input)
        {
            var result = parser.ParseNodeLabel(input);
            AssertNoErrors();
            return result;
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
            textState.behaviors.Count.Should().Be(1);
            textState.behaviors[0].order.Should().Be("11");
            textState.behaviors[0].triggers.Should().BeEquivalentTo(new string[] { "MY_EVENT" });
            textState.behaviors[0].guardCode.Should().Be(@"some_guard( ""my }str with spaces"" ) && blah");
            textState.behaviors[0].actionCode.Should().Be("my_action();");
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
            textState.behaviors.Count.Should().Be(2);
            textState.behaviors[0].order.Should().Be(null);
            textState.behaviors[0].triggers.Count.Should().Be(0);
            textState.behaviors[0].guardCode.Should().Be("true");
            textState.behaviors[0].actionCode.Trim().Should().Be("");

            textState.behaviors[1].order.Should().Be(null);
            textState.behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
            textState.behaviors[1].guardCode.Should().Be(null);
            textState.behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
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
            textState.behaviors.Count.Should().Be(1);
            textState.behaviors[0].order.Should().Be(null);
            textState.behaviors[0].triggers.Count.Should().Be(1);
            textState.behaviors[0].guardCode.Should().Be(null);
            textState.behaviors[0].actionCode.Trim().Should().Be("action_code(123);");
        }

        [Fact]
        public void DeIndentMultilineAction()
        {
            string input = @"
                OVEN_OFF
                event / { var += 3;
                    if (func(123))
                        stuff( func(8 * 2) );
                    if (true) {
                        a = ""hello there"";
                    }
                }
            ";
            var textState = (StateNode)ParseNodeWithNoErrors(input);
            textState.stateName.Should().Be("OVEN_OFF");
            textState.behaviors.Count.Should().Be(1);
            textState.behaviors[0].order.Should().Be(null);
            textState.behaviors[0].triggers.Count.Should().Be(1);
            textState.behaviors[0].guardCode.Should().Be(null);
            textState.behaviors[0].actionCode.Should().Be("var += 3;\r\n" +
                                                          "if (func(123))\r\n" +
                                                          "    stuff( func(8 * 2) );\r\n" +
                                                          "if (true) {\r\n" +
                                                          "    a = \"hello there\";\r\n" +
                                                          "}"
                                                          );
        }

        [Fact]
        public void EdgeDeIndentMultilineAction()
        {
            string input = @"
                event / { var += 3;
                    if (func(123))
                        stuff( func(8 * 2) );
                    if (true) {
                        a = ""hello there"";
                    }
                }
            ";
            var behaviors = parser.ParseEdgeLabel(input);
            AssertNoErrors();
            behaviors.Count.Should().Be(1);
            behaviors[0].order.Should().Be(null);
            behaviors[0].triggers.Count.Should().Be(1);
            behaviors[0].guardCode.Should().Be(null);
            behaviors[0].actionCode.Should().Be("var += 3;\r\n" +
                                                "if (func(123))\r\n" +
                                                "    stuff( func(8 * 2) );\r\n" +
                                                "if (true) {\r\n" +
                                                "    a = \"hello there\";\r\n" +
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
            textState.behaviors.Count.Should().Be(2);
            textState.behaviors[0].order.Should().Be(null);
            textState.behaviors[0].triggers.Count.Should().Be(0);
            textState.behaviors[0].guardCode.Should().Be("true");
            textState.behaviors[0].actionCode.Trim().Should().Be("");

            textState.behaviors[1].order.Should().Be(null);
            textState.behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
            textState.behaviors[1].guardCode.Should().Be(null);
            textState.behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
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
            textState.behaviors.Count.Should().Be(2);
            textState.behaviors[0].order.Should().Be(null);
            textState.behaviors[0].triggers.Count.Should().Be(0);
            textState.behaviors[0].guardCode.Should().Be("true");
            textState.behaviors[0].actionCode.Trim().Should().Be("");

            textState.behaviors[1].order.Should().Be(null);
            textState.behaviors[1].triggers.Should().BeEquivalentTo(new string[] { "event" });
            textState.behaviors[1].guardCode.Should().Be(null);
            textState.behaviors[1].actionCode.Trim().Should().Be("action_code(123);");
        }

        [Fact]
        public void StatemachineNode()
        {
            string input = @"
                $STATEMACHINE : MicrowaveSm
            ";
            var node = (StateMachineNode)ParseNodeWithNoErrors(input);
            node.name.Should().Be("MicrowaveSm");
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
    }
}
