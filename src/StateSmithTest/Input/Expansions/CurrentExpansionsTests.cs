#nullable enable

using Xunit;
using StateSmith.Input.Expansions;
using FluentAssertions;
using StateSmith.Output.Gil;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig;

namespace StateSmithTest.Input.Expansions;

public class CurrentExpansionsTests
{
    private readonly WrappingExpander wrappingExpander;
    UserExpansionScriptBases userExpansionScriptBases = new();

    public CurrentExpansionsTests()
    {
        var expander = new Expander();
        
        ExpanderFileReflection expanderFileReflection = new(expander, userExpansionScriptBases);
        expanderFileReflection.AddAllExpansions(new MyExpansions());
        wrappingExpander = new WrappingExpander(expander, userExpansionScriptBases);
    }

    private class MyExpansions : UserExpansionScriptBase
    {
        string var_a => "expanded_var_a";
        string print(string arg) => $"""some_printer("{arg}")""";
        string print2(string arg) => $"""some_printer2({arg})""";
        string log() => $"""
            log("State: {CurrentNamedVertex.Name}, trigger: {CurrentTrigger}")
            """;
    }

    [Fact]
    public void TestCurrentNamedVertext_CurrentTrigger()
    {
        var state = new State("S1");
        var behavior = state.AddBehavior(new Behavior(trigger:"enter", actionCode: "log();"));
        
        userExpansionScriptBases.UpdateNamedVertex(state);
        userExpansionScriptBases.UpdateCurrentTrigger("enter");

        var code = wrappingExpander.ExpandWrapActionCode(behavior);
        code.Should().Be($"""{GilCreationHelper.GilFuncName_EchoStringStatement}("log(\"State: S1, trigger: enter\");");""");
    }
}
