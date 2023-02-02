using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using Xunit.Abstractions;
using StateSmith.Input.Antlr4;

namespace StateSmithTest.Antlr;

public class EntryExitParseTests : CommonTestHelper
{
    public EntryExitParseTests(ITestOutputHelper output)
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

    ////////////////////////////

    [Fact]
    public void ViaEntry_Valid()
    {
        string input = @"
        EVENT via entry 1
        ";
        var edge = parser.ParseEdgeLabel(input);
        edge[0].viaEntry.Should().Be("1");
        edge[0].viaExit.Should().BeNull();
        AssertNoErrors();
    }

    [Fact]
    public void ViaEntry_ValidWithAllTheStuff()
    {
        string input = @"
        EVENT [guard] / action_code(); via entry MY_ENTRY_POINT
        ";
        var edge = parser.ParseEdgeLabel(input);
        edge[0].viaEntry.Should().Be("MY_ENTRY_POINT");
        edge[0].viaExit.Should().BeNull();
        AssertNoErrors();
    }

    [Fact]
    public void ViaEntry_ValidWithAllTheStuff2()
    {
        string input = @"
        EVENT [guard] / { action_code(); } via exit MY_ENTRY_POINT
        ";
        var edge = parser.ParseEdgeLabel(input);
        edge[0].viaExit.Should().Be("MY_ENTRY_POINT");
        edge[0].viaEntry.Should().BeNull();
        AssertNoErrors();
    }

    [Fact]
    public void ViaExit_Valid()
    {
        string input = @"
        EVENT via exit finished_normally
        ";
        var edge = parser.ParseEdgeLabel(input);
        edge[0].viaExit.Should().Be("finished_normally");
        edge[0].viaEntry.Should().BeNull();
        AssertNoErrors();
    }

    [Fact]
    public void ViaEntryExit_Valid()
    {
        string input = @"
        EVENT via exit finished_normally   via entry start_at_b
        ";
        var edge = parser.ParseEdgeLabel(input);
        edge[0].viaExit.Should().Be("finished_normally");
        edge[0].viaEntry.Should().Be("start_at_b");
        AssertNoErrors();
    }

    [Fact]
    public void ViaEntry_InvalidMultiple()
    {
        string input = "EVENT via entry 1 via entry 2";
        Assert.Throws<ArgumentException>(() => { parser.ParseEdgeLabel(input); });
    }

    [Fact]
    public void ViaExit_InvalidMultiple()
    {
        string input = "EVENT via exit 1 via exit 2";
        Assert.Throws<ArgumentException>(() => { parser.ParseEdgeLabel(input); });
    }
}
