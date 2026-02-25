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
    const string OPEN_BRACE = "#123;";  // mermaid ascii escape for '{'
    const string CLOSE_BRACE = "#125;"; // mermaid ascii escape for '}'
    const string COLON_SUB = "꞉";
    const string SEMI_COLON_SUB = ";";
    const string BACKSLASH = "#92;";    // mermaid ascii escape for '\'
    const string HASH = "#35;"; // mermaid ascii escape for '#'
    const string SINGLE_QUOTE = "&#39;"; // mermaid ascii escape for `'`
    const string DOUBLE_QUOTE = "&quot;"; // mermaid ascii escape for '"'
    const string LT = "#60;"; // mermaid ascii escape for '<'
    const string GT = "#62;"; // mermaid ascii escape for '>'
    const string AMP = "&amp;"; // mermaid ascii escape for '&'

    public static string Trigger(string trigger)
    {
        return $$"""<span class='syntax-trigger-name'>{{trigger}}</span>""";
    }

    public static string Action(string code)
    {
        return $$"""<span class='syntax-action-start-end'>/ { </span><span class='syntax-action-code'>{{code}}</span><span class='syntax-action-start-end'> }</span>""";
    }

    public static string Guard(string code)
    {
        return $$"""<span class='syntax-guard-bracket'>[</span><span class='syntax-guard-code'>{{code}}</span><span class='syntax-guard-bracket'>]</span>""";
    }

    [Fact]
    public void SemiAndFullColons()
    {
        const string input  =  "a++; Class::b++;";
        const string expect = $"a++{SEMI_COLON_SUB} Class{COLON_SUB}{COLON_SUB}b++{SEMI_COLON_SUB}";

        // test once fully explicit
        Test(trigger: "do", actionCode: input, expected: $$"""<span class='syntax-trigger-name'>do</span> <span class='syntax-action-start-end'>/ { </span><span class='syntax-action-code'>a++; Class꞉꞉b++;</span><span class='syntax-action-start-end'> }</span>""");

        // confirm that helpers match explicit expectation above
        Test(trigger: "do", actionCode: input, expected: $"""{Trigger("do")} {Action(expect)}""");
    }

    [Fact]
    public void ActionNewlines()
    {
        const string input  = "a = \nb;";
        const string expect = "a = <br>b;";
        Test(trigger: "Ev1", actionCode: input, expected: $"""{Trigger("Ev1")} {Action(expect)}""");
    }

    [Fact]
    public void HashSymbol()
    {
        const string input  = "log('###')";
        const string expect = $"""log({SINGLE_QUOTE}{HASH}{HASH}{HASH}{SINGLE_QUOTE})""";
        Test(trigger: "Ev1", actionCode: input, expected: $"""{Trigger("Ev1")} {Action(expect)}""");
    }

    [Fact]
    public void Braces()
    {
        const string input  =    "if(true){}";
        const string expect = $"""if(true){OPEN_BRACE}{CLOSE_BRACE}""";
        Test(trigger: "Ev1", actionCode: input, expected: $"""{Trigger("Ev1")} {Action(expect)}""");
    }

    [Fact]
    public void Xml()
    {
        const string input  =  """log("<xml>");""";
        const string expect = $"""log({DOUBLE_QUOTE}{LT}xml{GT}{DOUBLE_QUOTE});""";
        Test(trigger: "Ev1", actionCode: input, expected: $"""{Trigger("Ev1")} {Action(expect)}""");
    }

    [Fact]
    public void ActionEscapedNewlineInStringAndQuotes()
    {
        const string input  =  """log("Hello\nWorld");""";
        const string expect = $"""log({DOUBLE_QUOTE}Hello{BACKSLASH}nWorld{DOUBLE_QUOTE});""";
        Test(trigger: "Ev1", actionCode: input, expected: $"""{Trigger("Ev1")} {Action(expect)}""");
    }

    [Fact]
    public void GuardNewlines()
    {
        const string input  = "a > 10 &&\nb < 20";
        const string expect = $"""a {GT} 10 {AMP}{AMP}<br>b {LT} 20""";

        // explicit guard code first time
        Test(trigger: "Ev1", guardCode: input, expected: $$"""{{Trigger("Ev1")}} <span class='syntax-guard-bracket'>[</span><span class='syntax-guard-code'>a #62; 10 &amp;&amp;<br>b #60; 20</span><span class='syntax-guard-bracket'>]</span> {{Action("")}}""");

        // validate helpers against explicit above
        Test(trigger: "Ev1", guardCode: input, expected: $$"""{{Trigger("Ev1")}} {{Guard(expect)}} {{Action("")}}""");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/355
    /// </summary>
    [Fact]
    public void AlwaysShowActionCode_355()
    {
        Test(trigger: "BLUE", expected: $$"""{{Trigger("BLUE")}} {{Action("")}}""");
    }

    private void Test(string? trigger = null, string guardCode = "", string actionCode = "", string expected = "")
    {
        var b = new Behavior(guardCode: guardCode, actionCode: actionCode);

        if (trigger != null)
        {
            b.AddTrigger(trigger);
        }

        gen.BehaviorToMermaidLabel(b).ShouldBeShowDiff(expected);
    }
}
