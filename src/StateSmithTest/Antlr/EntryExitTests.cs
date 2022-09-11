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


namespace StateSmithTest.Antlr;

public class EntryExitTests : CommonTestHelper
{
    public EntryExitTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void ExitPointLabels_Valid()
    {
        ParseExitPointWithLabel(@"exit:1", expected_label: "1");
        ParseExitPointWithLabel(@"exit: 1", expected_label: "1");
        ParseExitPointWithLabel(@"exit : 1", expected_label: "1");
        ParseExitPointWithLabel(@"exit : some_THING_342", expected_label: "some_THING_342");
    }

    [Fact]
    public void EntryPointLabels_Valid()
    {
        ParseEntryPointWithLabel(@"entry:1", expected_label: "1");
        ParseEntryPointWithLabel(@"entry: 1", expected_label: "1");
        ParseEntryPointWithLabel(@"entry : 1", expected_label: "1");
        ParseEntryPointWithLabel(@"entry : some_THING_342", expected_label: "some_THING_342");
    }

    [Fact]
    public void ExitPointLabels_Invalid()
    {
        var node = (ExitPointNode)ParseNodeWithAtLeastOneError("exit:");

        // test a valid one after to make sure test state not getting kept
        ParseExitPointWithLabel(@"exit:1", expected_label: "1");
    }


    private void ParseExitPointWithLabel(string input, string expected_label)
    {
        var node = (ExitPointNode)ParseNodeWithNoErrors(input);
        node.label.Should().Be(expected_label);
    }

    private void ParseEntryPointWithLabel(string input, string expected_label)
    {
        var node = (EntryPointNode)ParseNodeWithNoErrors(input);
        node.label.Should().Be(expected_label);
    }

    [Fact]
    public void ExitNoKeywordConflict()
    {
        string input = @"MY_STATE
        exit / { }
        exit / exit = 45; exit();
        ";
        var node = (StateNode)ParseNodeWithNoErrors(input);
    }

    [Fact]
    public void ExitNoKeywordConflict_StateName()
    {
        string input = @"EXIT";
        var node = (StateNode)ParseNodeWithNoErrors(input);
        node.stateName.Should().Be("EXIT");
    }
}
