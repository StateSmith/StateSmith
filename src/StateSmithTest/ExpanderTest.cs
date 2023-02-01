using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Input.Expansions;
using StateSmith.Input.Antlr4;

namespace StateSmithTest
{
    /*
     A bit torn on direction. Using a property (like below) makes for easier reuse, but also defines a shadow function called `get_time()` which can conflict
        string time => "_get_time();";
        string get_time() => "system_get_time()";   //CLASHES with `time` property!

    Using fields:
        pros: simple, no name clashing
        cons: can't reference other expansions unless they are marked as static

            string time2 = "get_time();";
            string time3 = time2;   //fail!

            //more verbose
            static string time2 = "get_time();";
            static string time3 = time2;
    */

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable RCS1018 // Add accessibility modifiers (or vice versa).
#pragma warning disable RCS1213 // Remove unused member declaration.
#pragma warning disable IDE0044 // Add readonly modifier
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
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore RCS1018 // Add accessibility modifiers (or vice versa).
#pragma warning restore RCS1213 // Remove unused member declaration.


    public class ExpanderTest
    {
        [Fact]
        public void Test1()
        {
            Expander expander = new Expander();
            var userExpansions = new ExpansionsExample();
            ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
            userExpansions.varsPath = "sm->vars.";
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
            expander.TryExpandFunctionExpansion("func", new string[] { }).Should().Be("123");
        }
    }

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
            ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
            expanderFileReflection.AddAllExpansions(new MyExpansions());
            ExpandingVisitor.ParseAndExpandCode(expander, "mult( mult( 2, 4 ), mult(1, 10))").Should().Be("80");
        }

        [Fact]
        public void ShouldNotAffectMethods()
        {
            var expander = new Expander();
            ExpanderFileReflection expanderFileReflection = new ExpanderFileReflection(expander);
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
}
