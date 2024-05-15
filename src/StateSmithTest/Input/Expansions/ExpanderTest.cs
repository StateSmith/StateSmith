using Xunit;
using FluentAssertions;
using StateSmith.Input.Expansions;

namespace StateSmithTest.Input.Expansions;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier

public class ExpanderTest
{
    class ExpansionsExample : UserExpansionScriptBase
    {
        string time => get_time_;

        //`time` property also creates `get_time` method which prevents us from creating `get_time` property
        //so we use a custom attribute to set the name we want.
        [ExpansionName("get_time")]
        string get_time_ => "system_get_time()";

        string set_mode(string enum_name) => $"set_mode(ENUM_PREFIX_{enum_name})";

        string hit_count = "sm->vars." + AutoNameCopy();   //`AutoNameToken` maps to name of field. Result: "sm->vars.hit_count"
        string jump_count => AutoVarName();

        string func() => "123";
    }


    [Fact]
    public void Test1()
    {
        Expander expander = new();
        TestExpander(expander);
    }

    private static void TestExpander(Expander expander)
    {
        var userExpansions = new ExpansionsExample();
        ExpanderFileReflection expanderFileReflection = new(expander, new());
        userExpansions.VarsPath = "sm->vars.";
        expanderFileReflection.AddAllExpansions(userExpansions);

        expander.GetVariableNames().Should().BeEquivalentTo(new string[] {
            "time",
            "get_time",
            "hit_count",
            "jump_count",
        });
        expander.TryExpandVariableExpansion("time").Should().Be("system_get_time()");
        expander.TryExpandVariableExpansion("get_time").Should().Be("system_get_time()");
        expander.TryExpandVariableExpansion("hit_count").Should().Be("sm->vars.hit_count");
        expander.TryExpandVariableExpansion("jump_count").Should().Be("sm->vars.jump_count");

        expander.GetFunctionNames().Should().BeEquivalentTo(new string[] {
            "set_mode",
            "func",
        });
        expander.TryExpandFunctionExpansion("set_mode", new string[] { "GRUNKLE" }).Should().Be("set_mode(ENUM_PREFIX_GRUNKLE)");
        expander.TryExpandFunctionExpansion("set_mode", new string[] { "STAN" }).Should().Be("set_mode(ENUM_PREFIX_STAN)");
        expander.TryExpandFunctionExpansion("func", System.Array.Empty<string>()).Should().Be("123");
    }

    [Fact]
    public void TrackingExpanderTest1()
    {
        TrackingExpander expander = new();
        TestExpander(expander);

        expander.AttemptedVariableExpansions.Should().BeEquivalentTo(new string[]
        {
            "time",
            "get_time",
            "hit_count",
            "jump_count",
        });

        expander.AttemptedFunctionExpansions.Should().BeEquivalentTo(new string[]
        {
            "set_mode",
            "func",
        });

        // it should also track failed expansions
        expander.TryExpandVariableExpansion("not_expanded_var").Should().Be("not_expanded_var");
        expander.AttemptedVariableExpansions.Should().BeEquivalentTo(new string[]
        {
            "time",
            "get_time",
            "hit_count",
            "jump_count",
            "not_expanded_var",
        });

        expander.TryExpandFunctionExpansion("not_expanded_func", new string[] { "GRUNKLE" }).Should().Be("not_expanded_func");
        expander.AttemptedFunctionExpansions.Should().BeEquivalentTo(new string[]
        {
            "set_mode",
            "func",
            "not_expanded_func",
        });

        // check failed expansions
        expander.FailedVariableExpansions.Should().BeEquivalentTo(new string[]
        {
            "not_expanded_var",
        });

        expander.FailedFunctionExpansions.Should().BeEquivalentTo(new string[]
        {
            "not_expanded_func",
        });
    }
}
