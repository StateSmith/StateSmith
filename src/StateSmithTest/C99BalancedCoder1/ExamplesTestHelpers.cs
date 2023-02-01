using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.output.C99BalancedCoder1;
using StateSmith.Runner;

namespace StateSmithTest
{
    class ExamplesTestHelpers
    {
        public static string TestInputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "../../../test-input/";
        public static string ExamplesInputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "../../../../../examples/";

        public static Compiler SetupTiny2Sm()
        {
            const string relativePath = "Tiny2.graphml";
            return SetupAndValidateCompilerForTestInputFile(relativePath).compiler;
        }

        public static CompilerRunner SetupAndValidateCompilerForTestInputFile(string relativePath)
        {
            CompilerRunner compilerRunner = CreateCompilerForTestInputFile(relativePath);

            compilerRunner.FinishRunningCompiler();

            return compilerRunner;
        }

        public static CompilerRunner CreateCompilerForTestInputFile(string relativePath)
        {
            string filepath = TestInputDirectoryPath + relativePath;
            CompilerRunner compilerRunner = new();
            compilerRunner.CompileYedFileNodesToVertices(filepath);
            return compilerRunner;
        }

        public static CodeGenContext SetupCtxForTiny2Sm()
        {
            var compiler = ExamplesTestHelpers.SetupTiny2Sm();
            var sm = compiler.rootVertices.Single().As<Statemachine>();
            return new CodeGenContext(sm);
        }

        public static CodeGenContext SetupCtxForSimple1()
        {
            CompilerRunner compilerRunner = new();

            var sm = new Statemachine("Simple1");
            var s1 = sm.AddChild(new State(name: "s1"));
            var s1_1 = s1.AddChild(new State(name: "s1_1"));

            var initialStateVertex = sm.AddChild(new InitialState());
            initialStateVertex.AddTransitionTo(s1);

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

            compilerRunner.SetStateMachineRoot(sm);
            compilerRunner.FinishRunningCompiler();

            return new CodeGenContext(sm);
        }
    }
}