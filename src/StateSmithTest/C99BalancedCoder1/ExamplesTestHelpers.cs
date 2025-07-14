using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmithTest
{
    class ExamplesTestHelpers
    {
        public static string TestInputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "../../../test-input/";
        public static string ExamplesInputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "../../../../../examples/";

        public static InputSmBuilder SetupTiny2Sm()
        {
            const string relativePath = "Tiny2.graphml";
            return SetupAndValidateCompilerForTestInputFile(relativePath);
        }

        public static InputSmBuilder SetupAndValidateCompilerForTestInputFile(string relativePath)
        {
            InputSmBuilder inputSmBuilder = CreateCompilerForTestInputFile(relativePath);

            inputSmBuilder.FinishRunning();

            return inputSmBuilder;
        }

        public static InputSmBuilder CreateCompilerForTestInputFile(string relativePath)
        {
            string filepath = TestInputDirectoryPath + relativePath;
            IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
            InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
            inputSmBuilder.ConvertYedFileNodesToVertices(filepath);
            return inputSmBuilder;
        }

        //public static CodeGenContext SetupCtxForTiny2Sm()
        //{
        //    var diagramToSmConverter = ExamplesTestHelpers.SetupTiny2Sm().diagramToSmConverter;
        //    var sm = diagramToSmConverter.rootVertices.Single().As<StateMachine>();
        //    return new CodeGenContext(sm);
        //}

        //public static CodeGenContext SetupCtxForSimple1()
        //{
        //    InputSmBuilder inputSmBuilder = new();

        //    var sm = new StateMachine("Simple1");
        //    var s1 = sm.AddChild(new State(name: "s1"));
        //    var s1_1 = s1.AddChild(new State(name: "s1_1"));

        //    var initialStateVertex = sm.AddChild(new InitialState());
        //    initialStateVertex.AddTransitionTo(s1);

        //    sm.AddBehavior(new Behavior()
        //    {
        //        triggers = new List<string>() { "do" }
        //    });

        //    s1.AddBehavior(new Behavior()
        //    {
        //        triggers = new List<string>() { "EV1", "do" }
        //    });
        //    s1_1.AddBehavior(new Behavior()
        //    {
        //        triggers = new List<string>() { "enter", "exit", "ZIP" }
        //    });

        //    inputSmBuilder.SetStateMachineRoot(sm);
        //    inputSmBuilder.FinishRunning();

        //    return new CodeGenContext(sm);
        //}
    }
}
