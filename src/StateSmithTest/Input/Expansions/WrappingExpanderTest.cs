#nullable enable

using Xunit;
using StateSmith.Input.Expansions;
using FluentAssertions;
using StateSmith.Output.Gil;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig;
using FluentAssertions.Execution;

namespace StateSmithTest.Input.Expansions;

// See https://github.com/StateSmith/StateSmith/wiki/Z:-mixing-GIL-and-user-code
public class WrappingExpanderTest
{
    private readonly WrappingExpander wrappingExpander;

    public WrappingExpanderTest()
    {
        var expander = new Expander();
        UserExpansionScriptBases userExpansionScriptBases = new();
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
            log("State: {CurrentNamedVertex.Name}, trigger: {CurrentTrigger}");
            """;
    }

    [Fact]
    public void ActionCodeBesideGil()
    {
        var code = ExpandWrapActionCode("var_a = 33; $gil(var_a = 45;) var_a++;");
        code.Should().Be($"""{GilCreationHelper.GilEchoStringVoidReturnFuncName}("expanded_var_a = 33; ");var_a = 45;{GilCreationHelper.GilEchoStringVoidReturnFuncName}(" expanded_var_a++;");""");
    }

    [Fact]
    public void ActionCodeWrappingGil()
    {
        var code = ExpandWrapActionCode("print($gil(var_a = 45;))");
        code.Should().Be($"""{GilCreationHelper.GilEchoStringVoidReturnFuncName}("some_printer(\"");var_a = 45;{GilCreationHelper.GilEchoStringVoidReturnFuncName}("\")");""");
    }

    [Fact]
    public void ActionCodeWithNewLinesGil()
    {
        var code = ExpandWrapActionCode("    var_a = 33;\n    var_a++;");
        code.Should().Be($"""
            {GilCreationHelper.GilEchoStringVoidReturnFuncName}("    expanded_var_a = 33;");
            {GilCreationHelper.GilEchoStringVoidReturnFuncName}("    expanded_var_a++;");
            """.ConvertLineEndingsToN());
    }

    [Fact]
    public void GuardCodeBeside()
    {
        var code = ExpandWrapGuardCode("var_a == \"255\"\n || $gil(var_a == 45) || var_a<25");
        code.Should().Be($"""{GilCreationHelper.GilVisitVarArgsBoolReturnFuncName}({GilCreationHelper.WrapRawCodeWithBoolReturn("expanded_var_a == \"255\"\n || ")},var_a == 45,{GilCreationHelper.WrapRawCodeWithBoolReturn(" || expanded_var_a<25")})""");
        code.Should().Be("""____GilNoEmit_VarArgsToBool(____GilNoEmit_EchoStringBool("expanded_var_a == \"255\"\n || "),var_a == 45,____GilNoEmit_EchoStringBool(" || expanded_var_a<25"))""");
    }

    [Fact]
    public void GuardCodeWrappingGil()
    {
        var code = ExpandWrapGuardCode("""print2("Result: " + $gil(var_a == 45))""");
        code.Should().Be($"""{GilCreationHelper.GilVisitVarArgsBoolReturnFuncName}({GilCreationHelper.WrapRawCodeWithBoolReturn("""some_printer2("Result: " + """)},var_a == 45,{GilCreationHelper.WrapRawCodeWithBoolReturn(")")})""");
        code.Should().Be("""____GilNoEmit_VarArgsToBool(____GilNoEmit_EchoStringBool("some_printer2(\"Result: \" + "),var_a == 45,____GilNoEmit_EchoStringBool(")"))""");
    }


    private string ExpandWrapActionCode(string actionCode)
    {
        var state = new State("S1");
        var behavior = state.AddBehavior(new Behavior(actionCode: actionCode));
        return wrappingExpander.ExpandWrapActionCode(behavior);
    }

    private string ExpandWrapGuardCode(string guardCode)
    {
        var state = new State("S1");
        var behavior = state.AddBehavior(new Behavior(guardCode: guardCode));
        return wrappingExpander.ExpandWrapGuardCode(behavior);
    }
}
