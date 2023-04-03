#nullable enable

using Xunit;
using StateSmith.Input.Expansions;
using FluentAssertions;
using StateSmith.Output.Gil;

namespace StateSmithTest.Input.Expansions;

// See https://github.com/StateSmith/StateSmith/wiki/Z:-mixing-GIL-and-user-code
public class WrappingExpanderTest
{
    private readonly WrappingExpander wrappingExpander;

    public WrappingExpanderTest()
    {
        var expander = new Expander();
        ExpanderFileReflection expanderFileReflection = new(expander);
        expanderFileReflection.AddAllExpansions(new MyExpansions());
        wrappingExpander = new WrappingExpander(expander);
    }

    private class MyExpansions : UserExpansionScriptBase
    {
        string var_a => "expanded_var_a";
        string print(string arg) => $"""some_printer("{arg}")""";
        string print2(string arg) => $"""some_printer2({arg})""";
    }

    [Fact]
    public void ActionCodeBeside()
    {
        var wrapFinalCode = PostProcessor.RmCommentOut;
        var code = wrappingExpander.ExpandWrapCode("var_a = 33; $gil(var_a = 45;) var_a++;", isGuard: false);
        code.Should().Be($"{wrapFinalCode("expanded_var_a = 33; ")}var_a = 45;{wrapFinalCode(" expanded_var_a++;")}");
        string expectedBeforePostProcess = "/*<<<<<rm2<<<<<expanded_var_a = 33; >>>>>rm2>>>>>*/var_a = 45;/*<<<<<rm2<<<<< expanded_var_a++;>>>>>rm2>>>>>*/";  //same as above, just so we can easily see it.
        code.Should().Be(expectedBeforePostProcess);
        PostProcessor.PostProcess(expectedBeforePostProcess).Should().Be("expanded_var_a = 33; var_a = 45; expanded_var_a++;");
    }

    [Fact]
    public void ActionCodeWrappingGil()
    {
        var wrapFinalCode = PostProcessor.RmCommentOut;
        var code = wrappingExpander.ExpandWrapCode("print($gil(var_a = 45;))", isGuard: false);
        code.Should().Be($"{wrapFinalCode("some_printer(\"")}var_a = 45;{wrapFinalCode("\")")}");
        code.Should().Be("""/*<<<<<rm2<<<<<some_printer(">>>>>rm2>>>>>*/var_a = 45;/*<<<<<rm2<<<<<")>>>>>rm2>>>>>*/"""); //same as above, just so we can easily see it.
    }

    [Fact]
    public void GuardCodeBeside()
    {
        var code = wrappingExpander.ExpandWrapCode("var_a == \"255\"\n || $gil(var_a == 45) || var_a<25", isGuard: true);
        code.Should().Be($"""{GilCreationHelper.GilVisitVarArgsBoolReturnFuncName}({GilCreationHelper.WrapRawCodeWithBoolReturn("expanded_var_a == \"255\"\n || ")},var_a == 45,{GilCreationHelper.WrapRawCodeWithBoolReturn(" || expanded_var_a<25")})""");
        code.Should().Be("""____GilNoEmit_VarArgsToBool(____GilNoEmit_EchoStringBool("expanded_var_a == \"255\"\n || "),var_a == 45,____GilNoEmit_EchoStringBool(" || expanded_var_a<25"))""");
    }

    [Fact]
    public void GuardCodeWrappingGil()
    {
        var code = wrappingExpander.ExpandWrapCode("""print2("Result: " + $gil(var_a == 45))""", isGuard: true);
        code.Should().Be($"""{GilCreationHelper.GilVisitVarArgsBoolReturnFuncName}({GilCreationHelper.WrapRawCodeWithBoolReturn("""some_printer2("Result: " + """)},var_a == 45,{GilCreationHelper.WrapRawCodeWithBoolReturn(")")})""");
        code.Should().Be("""____GilNoEmit_VarArgsToBool(____GilNoEmit_EchoStringBool("some_printer2(\"Result: \" + "),var_a == 45,____GilNoEmit_EchoStringBool(")"))""");
    }
}
