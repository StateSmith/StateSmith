using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.output.C99BalancedCoder1;

namespace StateSmithTest
{
    class ExamplesTestHelpers
    {
        public static Compiler SetupTiny2Sm()
        {
            const string filepath = "../../../test-input/Tiny2.graphml";
            Compiler compiler = new Compiler();
            compiler.CompileFile(filepath);
            compiler.rootVertices.Count.Should().Be(1);

            FinishSettingUpCompiler(compiler);

            return compiler;
        }

        private static void FinishSettingUpCompiler(Compiler compiler)
        {
            compiler.SetupRoots();
            compiler.FinalizeTrees();
            compiler.Validate();
        }

        public static CodeGenContext SetupCtxForTiny2Sm()
        {
            var compiler = ExamplesTestHelpers.SetupTiny2Sm();
            var sm = compiler.rootVertices.Single().As<Statemachine>();
            return new CodeGenContext(sm);
        }

        public static CodeGenContext SetupCtxForSimple1()
        {
            Compiler compiler = new Compiler();

            var sm = new Statemachine("Simple1");
            var s1 = sm.AddChild(new State(name: "s1"));
            var s1_1 = s1.AddChild(new State(name: "s1_1"));

            var initialStateVertex = sm.AddChild(new InitialState());
            initialStateVertex.AddTransitionTo(s1);

            compiler.rootVertices = new List<Vertex>() { sm };
            compiler.SetupRoots();

            sm.AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "do" }
            });

            s1.AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "EV1", "do" }
            });
            s1_1.AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "enter", "exit", "ZIP" }
            });

            FinishSettingUpCompiler(compiler);

            return new CodeGenContext(sm);
        }
    }
}