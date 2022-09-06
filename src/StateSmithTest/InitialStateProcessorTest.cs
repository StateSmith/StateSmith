using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.Compiling;

namespace StateSmithTest.InitialStateProcessor
{
    public class SimpleSimplification
    {
        Compiler compiler;
        Statemachine root;
        InitialState root_initial;
        State s1;
        State s2;
        InitialState s2_initial;
        State s2_1;

        public SimpleSimplification()
        {
            compiler = new Compiler();
            BuildTestGraph();
            compiler.rootVertices = new List<Vertex>() { root };
        }

        [Fact]
        public void SingleReplacement()
        {
            //before simplification
            compiler.SetupRoots();
            s1.Behaviors[0].TransitionTarget.Should().Be(s2);
            s2.IncomingTransitions.Count.Should().Be(1);
            s2.IncomingTransitions[0].OwningVertex.Should().Be(s1);
            s2.Children.Count.Should().Be(2, because: "should still have initial state");
            s2_1.IncomingTransitions.Count.Should().Be(1);

            //do simplification and check
            compiler.SimplifyInitialStates();
            s1.Behaviors[0].TransitionTarget.Should().Be(s2_1);

            s2.IncomingTransitions.Should().BeEmpty(because: "moved to initial state target");
            s2_1.IncomingTransitions.Count.Should().Be(1);
            s2_1.IncomingTransitions[0].OwningVertex.Should().Be(s1);

            s2.Children.Count.Should().Be(1, because: "initial state should be removed");
        }

        [Fact]
        public void ReplacementChain()
        {
            var s2_1_1 = s2_1.AddChild(new State("s2_1_1"));
            var s2_1_initial = s2_1.AddChild(new InitialState());
            s2_1_initial.AddTransitionTo(s2_1_1);

            //before simplification
            compiler.SetupRoots();
            s1.Behaviors[0].TransitionTarget.Should().Be(s2);
            s2.IncomingTransitions.Count.Should().Be(1);
            s2.IncomingTransitions[0].OwningVertex.Should().Be(s1);
            s2.Children.Count.Should().Be(2, because: "should still have initial state");
            s2_1.Children.Count.Should().Be(2, because: "should still have initial state");
            s2_1.IncomingTransitions.Count.Should().Be(1);

            //do simplification and check
            compiler.SimplifyInitialStates();
            s1.Behaviors[0].TransitionTarget.Should().Be(s2_1_1);

            s2.IncomingTransitions.Should().BeEmpty(because: "moved to initial state target");
            s2_1.IncomingTransitions.Should().BeEmpty(because: "moved to initial state target");

            s2_1_1.IncomingTransitions.Count.Should().Be(1);
            s2_1_1.IncomingTransitions[0].OwningVertex.Should().Be(s1);

            s2.Children.Count.Should().Be(1, because: "initial state should be removed");
            s2_1.Children.Count.Should().Be(1, because: "initial state should be removed");
        }

        private void BuildTestGraph()
        {
            root = new Statemachine(name: "root");
            root_initial = root.AddChild(new InitialState());
            {
                s1 = root.AddChild(new State(name: "s1"));

                s2 = root.AddChild(new State(name: "s2"));
                {
                    s2_1 = s2.AddChild(new State(name: "s2_1"));
                    s2_initial = s2.AddChild(new InitialState());
                }
            }

            root_initial.AddTransitionTo(s1);
            s1.AddTransitionTo(s2);
            s2_initial.AddTransitionTo(s2_1);
            s2_1.AddTransitionTo(s1);
        }
    }
}
