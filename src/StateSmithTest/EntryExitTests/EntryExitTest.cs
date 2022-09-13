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
        EntryPoint entryPoint2;
        ExitPoint exitPoint1;
        ExitPoint exitPoint2;
        NamedVertex stateA;
        NamedVertex stateB;
        NamedVertex group1;
        NamedVertex waitForAck;
        NamedVertex cancelled;
        NamedVertex finishedNormally;
        Behavior transitionToExtra;
        Behavior transitionToCancelled;

        public Validations()
        {
            compiler = ExamplesTestHelpers.CreateCompilerForTestInputFile("entry-exit/valid1.graphml");

            helper = new ValidationTestHelper{
                compiler = compiler,
            };

            sm = (Statemachine)compiler.rootVertices.Single();
            stateA = sm.Children.Descendant("STATE_A");
            stateB = sm.Children.Descendant("STATE_B");
            group1 = sm.Children.Descendant("GROUP1");
            waitForAck = sm.Children.Descendant("WAIT_FOR_ACK");
            cancelled = sm.Children.Descendant("CANCELLED");
            finishedNormally = sm.Children.Descendant("FINISHED_NORMALLY");
            entryPoint1 = sm.Children.Descendant("STATE_B").IncomingTransitions.Select(b => b.OwningVertex).OfType<EntryPoint>().Single();
            entryPoint2 = sm.Children.Descendant("STATE_A2").IncomingTransitions.Select(b => b.OwningVertex).OfType<EntryPoint>().Single();
            exitPoint1 = (ExitPoint)waitForAck.Behaviors.First(b => b.HasTransition()).TransitionTarget;
            exitPoint2 = (ExitPoint)stateB.Behaviors.First(b => b.HasTransition()).TransitionTarget;

            transitionToExtra = sm.Children.Descendant("EXTRA").IncomingTransitions[0];
            transitionToCancelled = cancelled.IncomingTransitions[0];
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
        public void ExitTransitionTargetsSelf()
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
            entryPoint2.label = entryPoint1.label;
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
    }
}
