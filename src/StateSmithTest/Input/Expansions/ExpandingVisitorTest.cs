using Xunit;
using FluentAssertions;
using StateSmith.Input.Expansions;
using StateSmith.Input.Antlr4;

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
    }

    [Fact]
    public void Recursive()
    {
        var expander = new Expander();
        ExpanderFileReflection expanderFileReflection = new(expander);
        expanderFileReflection.AddAllExpansions(new MyExpansions());
        ExpandingVisitor.ParseAndExpandCode(expander, "mult( mult( 2, 4 ), mult(1, 10))").Should().Be("80");
    }

    [Fact]
    public void ShouldNotAffectMethods()
    {
        var expander = new Expander();
        ExpanderFileReflection expanderFileReflection = new(expander);
        expanderFileReflection.AddAllExpansions(new MyExpansions());

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
    }
}
