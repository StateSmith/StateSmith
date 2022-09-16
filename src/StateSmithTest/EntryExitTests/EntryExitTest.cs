using Xunit;
using System.Linq;
using StateSmith.Compiling;
using FluentAssertions;
using StateSmith.compiler;
using System.Collections.Generic;

namespace StateSmithTest.EntryExitTests
{
    public class Validations
    {
        ValidationTestHelper helper;
        Compiler compiler;
        Statemachine sm;
        EntryPoint entryPoint1;
        EntryPoint entryPointA2;
        ExitPoint exitPoint1;
        ExitPoint exitPoint2;
        NamedVertex stateA;
        NamedVertex stateA2;
        NamedVertex state1;
        NamedVertex state2;
        NamedVertex state3;
        NamedVertex stateB;
        NamedVertex thruEnd1;
        NamedVertex thruEnd2;
        NamedVertex group1;
        NamedVertex waitForAck;
        NamedVertex cancelled;
        NamedVertex finishedNormally;
        Behavior transitionToExtra;
        Behavior transitionToCancelled;
        Behavior entryA2Transition;

        public Validations()
        {
            compiler = ExamplesTestHelpers.CreateCompilerForTestInputFile("entry-exit/valid1.graphml");

            helper = new ValidationTestHelper{
                compiler = compiler,
            };

            sm = (Statemachine)compiler.rootVertices.Single();
            stateA = sm.Children.Descendant("STATE_A");
            stateA2 = sm.Children.Descendant("STATE_A2");
            state1 = sm.Children.Descendant("STATE_1");
            state2 = sm.Children.Descendant("STATE_2");
            state3 = sm.Children.Descendant("STATE_3");
            stateB = sm.Children.Descendant("STATE_B");
            group1 = sm.Children.Descendant("GROUP1");
            waitForAck = sm.Children.Descendant("WAIT_FOR_ACK");
            cancelled = sm.Children.Descendant("CANCELLED");
            finishedNormally = sm.Children.Descendant("FINISHED_NORMALLY");
            thruEnd1 = sm.Children.Descendant("THRU_END_1");
            thruEnd2 = sm.Children.Descendant("THRU_END_2");

            entryPoint1 = sm.Children.Descendant("STATE_B").IncomingTransitions.Select(b => b.OwningVertex).OfType<EntryPoint>().Single();
            entryPointA2 = sm.Children.Descendant("STATE_A2").IncomingTransitions.Select(b => b.OwningVertex).OfType<EntryPoint>().Single();
            exitPoint1 = (ExitPoint)waitForAck.Behaviors.First(b => b.HasTransition()).TransitionTarget;
            exitPoint2 = (ExitPoint)stateB.Behaviors.First(b => b.HasTransition()).TransitionTarget;

            transitionToExtra = sm.Children.Descendant("EXTRA").IncomingTransitions[0];
            transitionToCancelled = cancelled.IncomingTransitions[0];
            entryA2Transition = entryPointA2.Behaviors[0];
        }

        [Fact]
        public void ParseFile()
        {
            ExamplesTestHelpers.FinishSettingUpCompilerAndValidate(compiler);
        }

        [Fact]
        public void EntryPointMustNotHaveChildren()
        {
            entryPoint1.AddChild(new State("S3"));
            helper.ExpectVertexValidationException("An entry point cannot have child states.");
        }

        [Fact]
        public void EntryPointMustNotIncomingTransitions()
        {
            stateA.AddTransitionTo(entryPoint1).guardCode = "some code";
            helper.ExpectVertexValidationException("An entry point cannot have any incoming transitions.");
        }

        [Fact]
        public void EntryPointMustOnlyHaveOneBehavior()
        {
            entryPoint1.AddTransitionTo(stateA).guardCode = "some code";
            helper.ExpectVertexValidationException("An entry point must have only a single behavior (instead of 2) which is an outgoing transition (for now).");
        }

        [Fact]
        public void EntryPointMustHaveASingleTransition()
        {
            entryPoint1.Behaviors[0]._transitionTarget = null;
            helper.ExpectVertexValidationException("An entry point must have a single outgoing transition (for now).");
        }

        [Fact]
        public void EntryPointTransitionCantHaveEvent()
        {
            entryPoint1.Behaviors[0].triggers.Add("SOME_TRIGGER");
            helper.ExpectVertexValidationException("An entry point transition cannot have a trigger/event.");
        }

        [Fact]
        public void EntryPointTransitionCantHaveGuard()
        {
            entryPoint1.Behaviors[0].guardCode = "some guard";
            helper.ExpectVertexValidationException("An entry point transition cannot have any guard code (for now).");
        }

        [Fact]
        public void EntryPointTransitionsToParent()
        {
            entryA2Transition.RetargetTo(group1);
            helper.ExpectVertexValidationException($"An state's entry point transition cannot target itself.", () => compiler.SupportEntryExitPoints());
        }

        [Fact]
        public void EntryPointTransitionsToItself()
        {
            entryA2Transition.RetargetTo(entryPointA2);
            helper.ExpectVertexValidationException($"An entry point cannot have any incoming transitions.", () => compiler.SupportEntryExitPoints());
        }

        [Fact]
        public void EntryPointNoMatchingTransitions()
        {
            state1.Behaviors[0].viaEntry = null;
            helper.ExpectVertexValidationException($"No transitions match entry point with `via entry a2`.", () => compiler.SupportEntryExitPoints());
        }

        //-----------------------------

        [Fact]
        public void ExitPointMustNotHaveChildren()
        {
            exitPoint1.AddChild(new State("S3"));
            helper.ExpectVertexValidationException("An exit point cannot have child states.");
        }

        [Fact]
        public void ExitPointMustHaveAtleastOneIncomingTransition()
        {
            exitPoint1._incomingTransitions.Clear();
            helper.ExpectVertexValidationException("An exit point must at least one incoming transition (for now).");
        }

        [Fact]
        public void ExitPointCantHaveBehaviors()
        {
            exitPoint1.AddBehavior(new Behavior());
            helper.ExpectVertexValidationException("An exit point cannot have any behaviors/transitions (for now).");
        }

        //------------------------------

        [Fact]
        public void NoMatchingExitPointFound()
        {
            var oldLabel = exitPoint1.label;
            exitPoint1.label = "something_that_isnt_in_diagram";
            helper.ExpectBehaviorValidationException($"no matching exit point found with label `{oldLabel}`.");
        }

        [Fact]
        public void TooManyMatchingExitPointsFound()
        {
            exitPoint2.label = exitPoint1.label;
            helper.ExpectBehaviorValidationException($"multiple matching exit points found with label `{exitPoint1.label}`.");
        }

        [Fact]
        public void TooManyMatchingExitViasFound()
        {
            transitionToExtra.guardCode = "";
            transitionToExtra.viaExit = "cancelled";
            helper.ExpectVertexValidationException($"Too many transitions match exit point with `via exit cancelled`.", () => compiler.SupportEntryExitPoints());
        }

        [Fact]
        public void ZeroMatchingExitViasFound()
        {
            transitionToCancelled.viaExit = null;
            helper.ExpectVertexValidationException($"No transitions match exit point with `via exit cancelled`.", () => compiler.SupportEntryExitPoints());
        }

        [Fact]
        public void ExitTransitionTargetsParent()
        {
            transitionToCancelled.RetargetTo(group1);
            helper.ExpectBehaviorValidationException($"An exit transition must target something out of the state being exited.", () => compiler.SupportEntryExitPoints());
        }

        [Fact]
        public void ExitTransitionTargetsChild()
        {
            transitionToCancelled.RetargetTo(stateA);
            helper.ExpectBehaviorValidationException($"An exit transition must target something out of the state being exited.", () => compiler.SupportEntryExitPoints());
        }

        //------------------------------

        [Fact]
        public void NoMatchingEntryPointFound()
        {
            var oldLabel = entryPoint1.label;
            entryPoint1.label = "something_not_in_diagram";
            helper.ExpectBehaviorValidationException($"no matching entry point found with label `{oldLabel}`.");
        }

        [Fact]
        public void TooManyMatchingEntryPointsFound()
        {
            entryPointA2.label = entryPoint1.label;
            helper.ExpectBehaviorValidationException($"multiple matching entry points found with label `{entryPoint1.label}`.");
        }

        //-----------------------------

        [Fact]
        public void ExitPointFunctionality()
        {
            ExamplesTestHelpers.FinishSettingUpCompilerAndValidate(compiler);
            compiler.SupportEntryExitPoints();

            waitForAck.TransitionBehaviors().Single().TransitionTarget.Should().Be(cancelled);

            {
                var exitNormalTransition = stateB.TransitionBehaviors().Where(b => b.HasAtLeastOneTrigger()).Single();
                exitNormalTransition.triggers.Should().BeEquivalentTo(new List<string>() { "OK_PRESS" });
                exitNormalTransition.guardCode.Should().Be("guard1");
                exitNormalTransition.actionCode.Should().Be("normal_action1();normal_action_2();");
                exitNormalTransition.TransitionTarget.Should().Be(finishedNormally);
            }

            {
                var exitTimeoutTransition = stateB.TransitionBehaviors().Where(b => !b.HasAtLeastOneTrigger()).Single();
                exitTimeoutTransition.triggers.Count.Should().Be(0);
                exitTimeoutTransition.guardCode.Should().Be("timeout");
                exitTimeoutTransition.actionCode.Should().Be("timeout_action();");
                exitTimeoutTransition.TransitionTarget.Should().Be(finishedNormally);
            }
        }

        //-----------------------------

        [Fact]
        public void EntryPointFunctionality()
        {
            ExamplesTestHelpers.FinishSettingUpCompilerAndValidate(compiler);
            compiler.SimplifyInitialStates();
            compiler.SupportEntryExitPoints();

            {
                Behavior transition = state1.TransitionBehaviors().Single();
                transition.TransitionTarget.Should().Be(stateA2);
                transition.triggers.Should().BeEquivalentTo(new List<string>() { "EVENT" });
                transition.guardCode.Should().Be("value > 200");
                transition.actionCode.Should().Be("state1_tx_action(); a2_action();");
            }

            {
                Behavior transition = state2.TransitionBehaviors().Single();
                transition.TransitionTarget.Should().Be(stateB);
                transition.triggers.Should().BeEquivalentTo(new List<string>() { "EVENT2" });
                transition.guardCode.Should().Be("value > 50");
                transition.actionCode.Should().Be("state2_tx_action();");
            }


            {
                Behavior transition = stateA.TransitionBehaviors().Where(b => b.guardCode == "go_to_thru2").Single();
                transition.TransitionTarget.Should().Be(thruEnd2);
                transition.triggers.Should().BeEquivalentTo(new List<string>() { });
                transition.guardCode.Should().Be("go_to_thru2");
                transition.actionCode.Should().Be("");
            }

            {
                Behavior transition = stateA.TransitionBehaviors().Where(b => b.guardCode == "go_to_thru1").Single();
                transition.TransitionTarget.Should().Be(thruEnd1);
                transition.triggers.Should().BeEquivalentTo(new List<string>() { });
                transition.guardCode.Should().Be("go_to_thru1");
                transition.actionCode.Should().Be("");
            }

            {
                Behavior transition = state3.TransitionBehaviors().Single();
                transition.TransitionTarget.Should().Be(thruEnd2);
                transition.triggers.Should().BeEquivalentTo(new List<string>() { "EV33" });
                transition.guardCode.Should().BeNull();
                transition.actionCode.Should().Be("");
            }
        }
    }
}
