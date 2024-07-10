#nullable enable

using StateSmith.Output.Sim;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.Output.Sim;

/// <summary>
/// Tests for <see cref="MermaidGenerator.BehaviorToMermaidLabel(Behavior)"/>
/// </summary>
public class MermaidGenerator_BehaviorLabelTest
{
    MermaidGenerator gen = new(new());
    const string ACTION_START = "/";
    const string OPEN_BRACE = "#123;";  // mermaid ascii escape for '{'
    const string CLOSE_BRACE = "#125;"; // mermaid ascii escape for '}'
    const string SEMICOLON = "#59;";    // mermaid ascii escape for ';'
    const string BACKSLASH = "#92;";    // mermaid ascii escape for '\'

    [Fact]
    public void Semicolons()
    {
        Test(actionCode: "a++; b++;", expected: $"""{ACTION_START} {OPEN_BRACE} a++{SEMICOLON} b++{SEMICOLON} {CLOSE_BRACE}""");
    }

    [Fact]
    public void ActionNewlines()
    {
        Test(actionCode: "a++;\nb++;", expected: $"""{ACTION_START} {OPEN_BRACE} a++{SEMICOLON}\nb++{SEMICOLON} {CLOSE_BRACE}""");
    }

    [Fact]
    public void ActionNewlineInString()
    {
        Test(actionCode: """
            log('Hello
            World')
            """, expected: $"""{ACTION_START} {OPEN_BRACE} log('Hello\nWorld') {CLOSE_BRACE}""");
    }

    [Fact]
    public void ActionEscapedNewlineInString()
    {
        Test(actionCode: """log('Hello\nWorld')""", expected: $"""{ACTION_START} {OPEN_BRACE} log('Hello{BACKSLASH}nWorld') {CLOSE_BRACE}""");
    }

    [Fact]
    public void GuardNewlines()
    {
        Test(guardCode: "a > 10 &&\nb > 20", expected: $"""[a > 10 &&\nb > 20]""");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/355
    /// </summary>
    [Fact]
    public void AlwaysShowActionCode_355()
    {
        Test(trigger: "BLUE", expected: $"""BLUE {ACTION_START} {OPEN_BRACE}  {CLOSE_BRACE}""", alwaysShowActionCode: true);
    }

    private void Test(string? trigger = null, string guardCode = "", string actionCode = "", string expected = "", bool alwaysShowActionCode = false)
    {
        var b = new Behavior(guardCode: guardCode, actionCode: actionCode);

        if (trigger != null)
        {
            b.AddTrigger(trigger);
        }

        gen.BehaviorToMermaidLabel(b, alwaysShowActionCode: alwaysShowActionCode).ShouldBeShowDiff(expected);
    }
}
