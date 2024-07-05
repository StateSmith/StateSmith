using Xunit;
using FluentAssertions;
using StateSmith.Input.Expansions;
using StateSmith.Input.Antlr4;
using StateSmithTest.Output.UserConfig;
using StateSmithTest.Output;
using StateSmith.SmGraph.Validation;
using System.Text;

namespace StateSmithTest.Input.Expansions;

public class ExpandingVisitorTest
{
    private class MyExpansions : UserExpansionScriptBase
    {
        string mult(string a_str, string b_str)
        {
            int a = int.Parse(a_str);
            int b = int.Parse(b_str);
            return $"{a*b}";
        }

        string menu => AutoVarName();
    }

    [Fact]
    public void Recursive()
    {
        var expander = new Expander();
        ExpanderFileReflection expanderFileReflection = new(expander, new());

        expanderFileReflection.AddAllExpansions(new MyExpansions());
        ExpandingVisitor.ParseAndExpandCode(expander, "mult( mult( 2, 4 ), mult(1, 10))").Should().Be("80");
    }

    [Fact]
    public void ShouldNotAffectMethods()
    {
        var expander = new Expander();
        ExpanderFileReflection expanderFileReflection = new(expander, new());
        MyExpansions myExpansions = new();
        myExpansions.VarsPath = "sm->vars.";

        expanderFileReflection.AddAllExpansions(myExpansions);

        var code = "obj->mult(3, 10)";
        ExpandingVisitor.ParseAndExpandCode(expander, code).Should().Be(code);

        //detect member access with crazy whitespace
        code = @"obj
                        ->
                           mult(3, 10)";
        ExpandingVisitor.ParseAndExpandCode(expander, code).Should().Be(code);

        code = "obj.mult(3, 10)";
        ExpandingVisitor.ParseAndExpandCode(expander, code).Should().Be(code);

        code = "Obj::mult(3, 10)";
        ExpandingVisitor.ParseAndExpandCode(expander, code).Should().Be(code);

        //not really valid code in this case, but still should expand it
        code = "Obj mult(3, 10)";
        ExpandingVisitor.ParseAndExpandCode(expander, code).Should().Be("Obj 30");

        // https://github.com/StateSmith/StateSmith/issues/160
        ExpandingVisitor.ParseAndExpandCode(expander, """menu.setItems(["BUILD YOUR BOT!", "ABOUT"]);""").ShouldBeShowDiff("""sm->vars.menu.setItems(["BUILD YOUR BOT!", "ABOUT"]);""");
    }
}
