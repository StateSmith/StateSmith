using Xunit;
using System.Linq;
using StateSmith.Compiling;
// using FluentAssertions;
using StateSmith.compiler;

namespace StateSmithTest.EntryExitTests
{
    public class Validations
    {
        ValidationTestHelper helper;
        Compiler compiler;
        Statemachine sm;
        EntryPoint entryPoint;
        ExitPoint exitPoint;
        NamedVertex stateA;

        public Validations()
        {
            compiler = ExamplesTestHelpers.CreateCompilerForTestInputFile("entry-exit/valid1.graphml");

            helper = new ValidationTestHelper{
                compiler = compiler,
            };

            sm = (Statemachine)compiler.rootVertices.Single();
            stateA = sm.Children.Descendant("STATE_A");
            entryPoint = sm.Children.Descendant("STATE_B").IncomingTransitions.Select(b => b.OwningVertex).OfType<EntryPoint>().Single();
            exitPoint = (ExitPoint)sm.Children.Descendant("WAIT_FOR_ACK").Behaviors.First(b => b.HasTransition()).TransitionTarget;
        }

        [Fact]
        public void ParseFile()
        {
            ExamplesTestHelpers.FinishSettingUpCompilerAndValidate(compiler);
            // should not throw anything
        }

        [Fact]
        public void EntryPointMustNotHaveChildren()
        {
            entryPoint.AddChild(new State("S3"));
            helper.ExpectValidationException("An entry point cannot have child states.");
        }

        [Fact]
        public void EntryPointMustNotIncomingTransitions()
        {
            stateA.AddTransitionTo(entryPoint).guardCode = "some code";
            helper.ExpectValidationException("An entry point cannot have any incoming transitions.");
        }

        [Fact]
        public void EntryPointMustOnlyHaveOneBehavior()
        {
            entryPoint.AddTransitionTo(stateA).guardCode = "some code";
            helper.ExpectValidationException("An entry point must have only a single behavior (instead of 2) which is an outgoing transition (for now).");
        }

        [Fact]
        public void EntryPointMustHaveASingleTransition()
        {
            entryPoint.Behaviors[0]._transitionTarget = null;
            helper.ExpectValidationException("An entry point must have a single outgoing transition (for now).");
        }

        [Fact]
        public void EntryPointTransitionCantHaveEvent()
        {
            entryPoint.Behaviors[0].triggers.Add("SOME_TRIGGER");
            helper.ExpectValidationException("An entry point transition cannot have a trigger/event.");
        }

        [Fact]
        public void EntryPointTransitionCantHaveGuard()
        {
            entryPoint.Behaviors[0].guardCode = "some guard";
            helper.ExpectValidationException("An entry point transition cannot have any guard code (for now).");
        }

        //-----------------------------

        [Fact]
        public void ExitPointMustNotHaveChildren()
        {
            exitPoint.AddChild(new State("S3"));
            helper.ExpectValidationException("An exit point cannot have child states.");
        }

        [Fact]
        public void ExitPointMustHaveAtleastOneIncomingTransition()
        {
            exitPoint._incomingTransitions.Clear();
            helper.ExpectValidationException("An exit point must at least one incoming transition (for now).");
        }

        [Fact]
        public void ExitPointCantHaveBehaviors()
        {
            exitPoint.AddBehavior(new Behavior());
            helper.ExpectValidationException("An exit point cannot have any behaviors/transitions (for now).");
        }
    }
}
