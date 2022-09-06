using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.Input;
using StateSmith.compiler;

namespace StateSmithTest
{
    public class EventGatheringTests
    {
        private Statemachine BuildTestGraph(string rootName)
        {
            var sm = new Statemachine(name: rootName);

            var s1 = sm.AddChild(new State(name: "s1"));
            var s1_1 = s1.AddChild(new State(name: "s1_1"));

            var initialStateVertex = sm.AddChild(new InitialState());
            initialStateVertex.AddTransitionTo(s1);

            return sm;
        }

        [Fact]
        public void Test()
        {
            Compiler compiler = new Compiler();

            var sm1 = BuildTestGraph("Sm1");
            var sm2 = BuildTestGraph("Sm2");

            compiler.rootVertices = new List<Vertex>() { sm1, sm2 };
            compiler.SetupRoots();

            {
                sm1.Descendant("s1").AddBehavior(new Behavior()
                {
                    triggers = new List<string>() { "EV1", "do" }
                });
                sm1.Descendant("s1_1").AddBehavior(new Behavior()
                {
                    triggers = new List<string>() { "enter", "exit", "ZIP" }
                });
            }

            {
                sm2.Descendant("s1").AddBehavior(new Behavior()
                {
                    triggers = new List<string>() { "SM2_EV1", "do", "exit" }
                });
                sm2.Descendant("s1_1").AddBehavior(new Behavior()
                {
                    triggers = new List<string>() { "enter", "exit", "SM2_ZIP" }
                });
            }

            compiler.FinalizeTrees();

            sm1.GetEventListCopy().Should().Equal(new List<string>()
            {
                "do", "ev1", "zip"
            });

            sm2.GetEventListCopy().Should().Equal(new List<string>()
            {
                "do", "sm2_ev1", "sm2_zip"
            });
        }
    }
}
