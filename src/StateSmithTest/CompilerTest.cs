using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using StateSmith.SmGraph;
using static StateSmithTest.VertexTestHelper;
using StateSmith.Input.Expansions;
using StateSmith.Runner;
using StateSmith.Input.Antlr4;
using StateSmith.SmGraph.Validation;

namespace StateSmithTest
{

    public class CompilerTest
    {
        [Fact]
        public void Tiny1()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "Tiny1.graphml";

            InputSmBuilder inputSmBuilder = new();
            inputSmBuilder.ConvertYedFileNodesToVertices(filepath);
            inputSmBuilder.FindSingleStateMachine();
            var sm = inputSmBuilder.GetStateMachine();

            var Tiny1 = sm;
            var map = new NamedVertexMap(sm);
            State GetState(string stateName) => map.GetState(stateName);

            var A = GetState("A");
            var B = GetState("B");
            var C2 = GetState("C2");

            Tiny1.Depth.Should().Be(0);
            A.Depth.Should().Be(1);
            B.Depth.Should().Be(1);
            C2.Depth.Should().Be(1);

            ///////////
            Tiny1.Name.Should().Be("Tiny1");
            Tiny1.DiagramId.Should().Be("n0");
            Tiny1.Children.Count.Should().Be(4);
            Tiny1.Behaviors.Should().BeEmpty();

            ////////////
            var Tiny1InitialState = Tiny1.ChildType<InitialState>();
            Tiny1InitialState.Children.Should().BeEmpty();
            Tiny1InitialState.DiagramId.Should().Be("n0::n1");
            {
                var owner = Tiny1InitialState;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(1);
                behaviors[0].ShouldBeExactly(owningVertex: owner, transitionTarget: A, actionCode: "initial_action();", diagramId: "n0::e0");
            }


            ////////////
            Tiny1.Child("A").Should().Be(A);
            A.Children.Should().BeEmpty();
            A.DiagramId.Should().Be("n0::n0");
            {
                var owner = A;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(2);
                behaviors[0].ShouldBeExactly(owningVertex: owner, triggers: TriggerList("enter"), actionCode: "a_count += 1;");
                behaviors[1].ShouldBeExactly(owningVertex: owner, transitionTarget: B, triggers: TriggerList("EVENT1"), diagramId: "n0::e1");
            }

            ////////////
            Tiny1.Child("B").Should().Be(B);
            B.Children.Should().BeEmpty();
            B.DiagramId.Should().Be("n0::n2");
            {
                var owner = B;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(2);
                behaviors[0].ShouldBeExactly(owningVertex: owner, triggers: new List<string>() { "exit" }, actionCode: "b_exit();");
                behaviors[1].ShouldBeExactly(owningVertex: owner, transitionTarget: C2, triggers: TriggerList("EVENT2"), guardCode: "some_guard(200)", diagramId: "n0::e3");
            }


            ////////////
            Tiny1.Child("C2").Should().Be(C2);
            C2.Children.Should().BeEmpty();
            C2.DiagramId.Should().Be("n0::n3");
            {
                var owner = C2;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(2);
                behaviors[0].ShouldBeExactly(owningVertex: owner, triggers: new List<string>() { "EVENT2" }, actionCode: "set_mode(SAUCEY);");
                behaviors[1].ShouldBeExactly(owningVertex: owner, transitionTarget: A, triggers: TriggerList("EVENT1"), order: 1, diagramId: "n0::e2");
            }
        }


#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable RCS1018 // Add accessibility modifiers (or vice versa).
#pragma warning disable RCS1213 // Remove unused member declaration.
#pragma warning disable IDE0044 // Add readonly modifier
        private class Tiny1Expansions : UserExpansionScriptBase
        {
            string a_count => AutoVarName();

            string set_mode(string mode) => $"set_mode(MODE_{mode})";

            string some_guard(string count)
            {
                int int_count = int.Parse(count);

                if (int_count > 100)
                {
                    int_count += 1000;
                }

                return $"some_guard({int_count})";
            }

            string b_exit()
            {
                return "b_exit_count++";
            }
        }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore RCS1018 // Add accessibility modifiers (or vice versa).
#pragma warning restore RCS1213 // Remove unused member declaration.

        private static void ExpandBehavior(Expander expander, Behavior behavior)
        {
            if (behavior.actionCode != null)
            {
                behavior.actionCode = ExpandingVisitor.ParseAndExpandCode(expander, behavior.actionCode);
            }

            if (behavior.guardCode != null)
            {
                behavior.guardCode = ExpandingVisitor.ParseAndExpandCode(expander, behavior.guardCode);
            }
        }

        private static void ExpandAllBehaviors(Expander expander, StateMachine sm)
        {
            sm.VisitRecursively(vertex => {
                foreach (var behavior in vertex.Behaviors)
                {
                    ExpandBehavior(expander, behavior);
                }
            });
        }

        [Fact]
        public void ExpandedTiny1()
        {
            string filepath = ExamplesTestHelpers.TestInputDirectoryPath + "/Tiny1.graphml";

            InputSmBuilder inputSmBuilder = new();
            DiagramToSmConverter diagramToSmConverter = inputSmBuilder.diagramToSmConverter;
            var expander = new Expander();
            ExpanderFileReflection expanderFileReflection = new(expander, new());
            Tiny1Expansions userExpansions = new Tiny1Expansions();
            userExpansions.VarsPath = "sm->vars.";
            expanderFileReflection.AddAllExpansions(userExpansions);
            // could also add events
            // could also check for valid events in diagram
            inputSmBuilder.ConvertYedFileNodesToVertices(filepath);
            inputSmBuilder.FindSingleStateMachine();
            var sm = inputSmBuilder.GetStateMachine();

            ExpandAllBehaviors(expander, sm);

            diagramToSmConverter.rootVertices.Count.Should().Be(2);

            var map = new NamedVertexMap(sm);
            State GetState(string stateName) => map.GetState(stateName);

            var Tiny1 = sm;
            var A = GetState("A");
            var B = GetState("B");
            var C2 = GetState("C2");

            ///////////
            Tiny1.Name.Should().Be("Tiny1");
            Tiny1.DiagramId.Should().Be("n0");
            Tiny1.Children.Count.Should().Be(4);
            Tiny1.Behaviors.Should().BeEmpty();

            ////////////
            var Tiny1InitialState = Tiny1.ChildType<InitialState>();
            Tiny1InitialState.Children.Should().BeEmpty();
            Tiny1InitialState.DiagramId.Should().Be("n0::n1");
            {
                var owner = Tiny1InitialState;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(1);
                behaviors[0].ShouldBeExactly(owningVertex: owner, transitionTarget: A, actionCode: "initial_action();", diagramId: "n0::e0");
            }


            ////////////
            Tiny1.Child("A").Should().Be(A);
            A.Children.Should().BeEmpty();
            A.DiagramId.Should().Be("n0::n0");
            {
                var owner = A;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(2);
                behaviors[0].ShouldBeExactly(owningVertex: owner, triggers: TriggerList("enter"), actionCode: "sm->vars.a_count += 1;");
                behaviors[1].ShouldBeExactly(owningVertex: owner, transitionTarget: B, triggers: TriggerList("EVENT1"), diagramId: "n0::e1");
            }

            ////////////
            Tiny1.Child("B").Should().Be(B);
            B.Children.Should().BeEmpty();
            B.DiagramId.Should().Be("n0::n2");
            {
                var owner = B;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(2);
                behaviors[0].ShouldBeExactly(owningVertex: owner, triggers: new List<string>() { "exit" }, actionCode: "b_exit_count++;");
                behaviors[1].ShouldBeExactly(owningVertex: owner, transitionTarget: C2, triggers: TriggerList("EVENT2"), guardCode: "some_guard(1200)", diagramId: "n0::e3");
            }


            ////////////
            Tiny1.Child("C2").Should().Be(C2);
            C2.Children.Should().BeEmpty();
            C2.DiagramId.Should().Be("n0::n3");
            {
                var owner = C2;
                var behaviors = owner.Behaviors;
                behaviors.Count.Should().Be(2);
                behaviors[0].ShouldBeExactly(owningVertex: owner, triggers: new List<string>() { "EVENT2" }, actionCode: "set_mode(MODE_SAUCEY);");
                behaviors[1].ShouldBeExactly(owningVertex: owner, transitionTarget: A, triggers: TriggerList("EVENT1"), order: 1, diagramId: "n0::e2");
            }
        }
    }
    public class Tiny2Test
    {
        InputSmBuilder inputSmBuilder;

        StateMachine sm;
        State S1;
        State S1_1;
        State S1_1_1;
        State S1_1_2;
        State S1_2;
        State S2;

        public Tiny2Test()
        {
            inputSmBuilder = ExamplesTestHelpers.SetupTiny2Sm();

            sm = inputSmBuilder.GetStateMachine();
            var map = new NamedVertexMap(sm);
            State GetState(string stateName) => map.GetState(stateName);

            S1 = GetState("S1");
            S1_1 = GetState("S1_1");
            S1_1_1 = GetState("S1_1_1");
            S1_1_2 = GetState("S1_1_2");
            S1_2 = GetState("S1_2");
            S2 = GetState("S2");
        }

        [Fact]
        public void TestDepth()
        {
            sm.Depth.Should().Be(0); //root
            S1.Depth.Should().Be(1);
            S1_1.Depth.Should().Be(2);
            S1_1_1.Depth.Should().Be(3);
            S1_1_2.Depth.Should().Be(3);
            S1_2.Depth.Should().Be(2);
            S2.Depth.Should().Be(1);
        }

        [Fact]
        public void DepthAddChild()
        {
            var S3 = sm.AddChild(new State("S3"));
            S3.Depth.Should().Be(1);

            var S1_3 = S1.AddChild(new State("S1_3"));
            S1_3.Depth.Should().Be(2);

            var S1_1_2_1 = S1_1_2.AddChild(new State("S1_1_2_1"));
            S1_1_2_1.Depth.Should().Be(4);

            TestDepth();

            //ensure nothing changed after setup roots called again
            TestDepth();
            S3.Depth.Should().Be(1);
            S1_3.Depth.Should().Be(2);
            S1_1_2_1.Depth.Should().Be(4);
        }

        [Fact]
        public void PreventAddingChildTwice()
        {
            Action action = () => S1.AddChild(S2);

            action.Should().Throw<VertexValidationException>()
                .Where(e => e.Message.Contains("already has a parent"));
        }

        [Fact]
        public void DepthAddSubTree()
        {
            var TRoot = new State("TRoot");
            var T1 = TRoot.AddChild(new State("T1"));
            var T1_1 = T1.AddChild(new State("T1_1"));
            var T1_2 = T1.AddChild(new State("T1_2"));
            var T1_2_1 = T1_2.AddChild(new State("T1_2_1"));

            TRoot.Depth.Should().Be(0);
            T1.Depth.Should().Be(1);
            T1_1.Depth.Should().Be(2);
            T1_2.Depth.Should().Be(2);
            T1_2_1.Depth.Should().Be(3);

            S1_1_1.AddChild(TRoot);

            //Depths should be updated to start from depth of S1_1_1
            S1_1_1.Depth.Should().Be(3);
            TRoot.Depth.Should().Be(4 + 0);
            T1.Depth.Should().Be(4 + 1);
            T1_1.Depth.Should().Be(4 + 2);
            T1_2.Depth.Should().Be(4 + 2);
            T1_2_1.Depth.Should().Be(4 + 3);

        }
    }
}
