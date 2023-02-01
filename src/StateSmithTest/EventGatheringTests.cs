using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.Input;
using StateSmith.compiler;
using StateSmith.Runner;

namespace StateSmithTest
{
    public class EventGatheringTests
    {
        private static Statemachine BuildTestGraph(string rootName)
        {
            var sm = new Statemachine(name: rootName);

            var s1 = sm.AddChild(new State(name: "s1"));
            s1.AddChild(new State(name: "s1_1"));

            var initialStateVertex = sm.AddChild(new InitialState());
            initialStateVertex.AddTransitionTo(s1);

            return sm;
        }

        [Fact]
        public void Test()
        {
            var sm = BuildTestGraph("Sm1");
            CompilerRunner compilerRunner = new();
            compilerRunner.SetStateMachineRoot(sm);
            var map = new NamedVertexMap(sm);
            State GetState(string stateName) => map.GetState(stateName);

            GetState("s1").AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "EV1", "do" }
            });
            GetState("s1_1").AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "enter", "exit", "ZIP" }
            });

            compilerRunner.FinishRunningCompiler();

            sm.GetEventListCopy().Should().Equal(new List<string>()
            {
                "do", "ev1", "zip"
            });
        }

        [Fact]
        public void Test2()
        {
            var sm = BuildTestGraph("Sm2");
            CompilerRunner compilerRunner = new();
            compilerRunner.SetStateMachineRoot(sm);
            var map = new NamedVertexMap(sm);
            State GetState(string stateName) => map.GetState(stateName);

            GetState("s1").AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "SM2_EV1", "do", "exit" }
            });
            GetState("s1_1").AddBehavior(new Behavior()
            {
                triggers = new List<string>() { "enter", "exit", "SM2_ZIP" }
            });

            compilerRunner.FinishRunningCompiler();
            
            sm.GetEventListCopy().Should().Equal(new List<string>()
            {
                "do", "sm2_ev1", "sm2_zip"
            });
        }
    }
}
