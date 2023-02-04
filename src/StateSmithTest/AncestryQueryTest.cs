using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using StateSmith.Runner;

namespace StateSmithTest
{
    public class AncestryQueryTest
    {
        InputSmBuilder inputSmBuilder;

        StateMachine root;
        InitialState root_initialState;
        State S1;
        State S1_1;
        State S1_1_1;
        State S1_1_2;
        State S1_2;
        State S2;

        public AncestryQueryTest()
        {
            inputSmBuilder = ExamplesTestHelpers.SetupTiny2Sm();

            root = inputSmBuilder.GetStateMachine();
            var map = new NamedVertexMap(root);
            State GetState(string stateName) => map.GetState(stateName);

            root_initialState = root.ChildType<InitialState>();
            S1 = GetState("S1");
            S1_1 = GetState("S1_1");
            S1_1_1 = GetState("S1_1_1");
            S1_1_2 = GetState("S1_1_2");
            S1_2 = GetState("S1_2");
            S2 = GetState("S2");
        }


        [Fact]
        public void ContainsVertex()
        {
            ExpectContains(container: root, new List<Vertex>() { root, root_initialState, S1, S1_1, S1_1_1, S1_1_2, S1_2, S2 });

            ExpectContains(container: S1, new List<Vertex>() { S1, S1_1, S1_1_1, S1_1_2, S1_2 });
            ExpectNotContained(container: S1, new List<Vertex>() { root, root_initialState, S2 });

            ExpectContains(container: S1_1, new List<Vertex>() { S1_1, S1_1_1, S1_1_2, });
            ExpectNotContained(container: S1_1, new List<Vertex>() { root, root_initialState, S1, S1_2, S2 });

            ExpectContains(container: S1_1_1, new List<Vertex>() { S1_1_1, });
            ExpectNotContained(container: S1_1_1, new List<Vertex>() { root, root_initialState, S1, S1_1, S1_1_2, S1_2, S2 });

            ExpectContains(container: S1_1_2, new List<Vertex>() { S1_1_2, });
            ExpectNotContained(container: S1_1_2, new List<Vertex>() { root, root_initialState, S1, S1_1, S1_1_1, S1_2, S2 });

            ExpectContains(container: S1_2, new List<Vertex>() { S1_2, });
            ExpectNotContained(container: S1_2, new List<Vertex>() { root, root_initialState, S1, S1_1, S1_1_1, S1_1_2, S2 });

            ExpectContains(container: S2, new List<Vertex>() { S2, });
            ExpectNotContained(container: S2, new List<Vertex>() { root, root_initialState, S1, S1_1, S1_1_1, S1_1_2, S1_2 });
        }

        [Fact]
        public void FindLca()
        {
            root.FindLcaWith(new State("blah blah")).Should().Be(null, because: "there's no path between the two");

            root.FindLcaWith(root).Should().Be(null);
            root.FindLcaWith(root_initialState).Should().Be(root);
            root.FindLcaWith(S1).Should().Be(root);
            root.FindLcaWith(S1_1).Should().Be(root);
            root.FindLcaWith(S1_1_1).Should().Be(root);
            root.FindLcaWith(S1_1_2).Should().Be(root);
            root.FindLcaWith(S1_2).Should().Be(root);
            root.FindLcaWith(S2).Should().Be(root);

            root_initialState.FindLcaWith(root).Should().Be(root);
            root_initialState.FindLcaWith(root_initialState).Should().Be(root);
            root_initialState.FindLcaWith(S1).Should().Be(root);
            root_initialState.FindLcaWith(S1_1).Should().Be(root);
            root_initialState.FindLcaWith(S1_1_1).Should().Be(root);
            root_initialState.FindLcaWith(S1_1_2).Should().Be(root);
            root_initialState.FindLcaWith(S1_2).Should().Be(root);
            root_initialState.FindLcaWith(S2).Should().Be(root);

            S1.FindLcaWith(root).Should().Be(root);
            S1.FindLcaWith(root_initialState).Should().Be(root);
            S1.FindLcaWith(S1).Should().Be(root);
            S1.FindLcaWith(S1_1).Should().Be(S1);
            S1.FindLcaWith(S1_1_1).Should().Be(S1);
            S1.FindLcaWith(S1_1_2).Should().Be(S1);
            S1.FindLcaWith(S1_2).Should().Be(S1);
            S1.FindLcaWith(S2).Should().Be(root);

            S1_1.FindLcaWith(root).Should().Be(root);
            S1_1.FindLcaWith(root_initialState).Should().Be(root);
            S1_1.FindLcaWith(S1).Should().Be(S1);
            S1_1.FindLcaWith(S1_1).Should().Be(S1);
            S1_1.FindLcaWith(S1_1_1).Should().Be(S1_1);
            S1_1.FindLcaWith(S1_1_2).Should().Be(S1_1);
            S1_1.FindLcaWith(S1_2).Should().Be(S1);
            S1_1.FindLcaWith(S2).Should().Be(root);

            S1_1_1.FindLcaWith(root).Should().Be(root);
            S1_1_1.FindLcaWith(root_initialState).Should().Be(root);
            S1_1_1.FindLcaWith(S1).Should().Be(S1);
            S1_1_1.FindLcaWith(S1_1).Should().Be(S1_1);
            S1_1_1.FindLcaWith(S1_1_1).Should().Be(S1_1);
            S1_1_1.FindLcaWith(S1_1_2).Should().Be(S1_1);
            S1_1_1.FindLcaWith(S1_2).Should().Be(S1);
            S1_1_1.FindLcaWith(S2).Should().Be(root);

            S1_1_2.FindLcaWith(root).Should().Be(root);
            S1_1_2.FindLcaWith(root_initialState).Should().Be(root);
            S1_1_2.FindLcaWith(S1).Should().Be(S1);
            S1_1_2.FindLcaWith(S1_1).Should().Be(S1_1);
            S1_1_2.FindLcaWith(S1_1_1).Should().Be(S1_1);
            S1_1_2.FindLcaWith(S1_1_2).Should().Be(S1_1);
            S1_1_2.FindLcaWith(S1_2).Should().Be(S1);
            S1_1_2.FindLcaWith(S2).Should().Be(root);

            S1_2.FindLcaWith(root).Should().Be(root);
            S1_2.FindLcaWith(root_initialState).Should().Be(root);
            S1_2.FindLcaWith(S1).Should().Be(S1);
            S1_2.FindLcaWith(S1_1).Should().Be(S1);
            S1_2.FindLcaWith(S1_1_1).Should().Be(S1);
            S1_2.FindLcaWith(S1_1_2).Should().Be(S1);
            S1_2.FindLcaWith(S1_2).Should().Be(S1);
            S1_2.FindLcaWith(S2).Should().Be(root);

            S2.FindLcaWith(root).Should().Be(root);
            S2.FindLcaWith(root_initialState).Should().Be(root);
            S2.FindLcaWith(S1).Should().Be(root);
            S2.FindLcaWith(S1_1).Should().Be(root);
            S2.FindLcaWith(S1_1_1).Should().Be(root);
            S2.FindLcaWith(S1_1_2).Should().Be(root);
            S2.FindLcaWith(S1_2).Should().Be(root);
            S2.FindLcaWith(S2).Should().Be(root);
        }

        [Fact]
        public void FindPath()
        {
            {
                var path = root.FindTransitionPathTo(S1_1_2);
                path.toExit.Should().Equal(new List<Vertex>()
                {

                });
                path.leastCommonAncestor.Should().Be(root);
                path.toEnter.Should().Equal(new List<Vertex>() {
                    S1, S1_1, S1_1_2
                });
            }
            //reverse path
            {
                var path = S1_1_2.FindTransitionPathTo(root);
                path.toExit.Should().Equal(new List<Vertex>() {
                    S1_1_2, S1_1, S1
                });
                path.leastCommonAncestor.Should().Be(root);
                path.toEnter.Should().Equal(new List<Vertex>()
                {

                });
            }
            {
                var path = S1_1_1.FindTransitionPathTo(S1_1_2);
                path.toExit.Should().Equal(new List<Vertex>() {
                    S1_1_1
                });
                path.leastCommonAncestor.Should().Be(S1_1);
                path.toEnter.Should().Equal(new List<Vertex>() {
                    S1_1_2
                });
            }
            {
                var path = S1_1_1.FindTransitionPathTo(S2);
                path.toExit.Should().Equal(new List<Vertex>() {
                    S1_1_1, S1_1, S1
                });
                path.leastCommonAncestor.Should().Be(root);
                path.toEnter.Should().Equal(new List<Vertex>() {
                    S2
                });
            }
            {
                var path = S1_1_1.FindTransitionPathTo(S1_2);
                path.toExit.Should().Equal(new List<Vertex>() {
                    S1_1_1, S1_1
                });
                path.leastCommonAncestor.Should().Be(S1);
                path.toEnter.Should().Equal(new List<Vertex>() {
                    S1_2
                });
            }
        }

        private static void ExpectContains(Vertex container, List<Vertex> contained)
        {
            foreach (var v in contained)
            {
                if (!container.ContainsVertex(v))
                {
                    string vDescription = Describe(v);
                    container.ContainsVertex(v).Should().Be(true, because: $"{vDescription} is should be contained");
                }
            }
        }

        private static void ExpectNotContained(Vertex container, List<Vertex> notContained)
        {
            foreach (var v in notContained)
            {
                if (container.ContainsVertex(v))
                {
                    string vDescription = Describe(v);
                    container.ContainsVertex(v).Should().Be(false, because: $"{vDescription} should NOT be contained");
                }
            }
        }

        private static string Describe(Vertex v)
        {
            return VertexPathDescriber.Describe(v);
        }
    }
}
