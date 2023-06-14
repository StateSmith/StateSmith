using System.Collections.Generic;
using Xunit;
using StateSmith.SmGraph;
using System.Linq;

namespace StateSmithTest.PseudoStateTests
{
    public abstract class PseudoStateValidationTestHelper : ValidationTestHelper
    {
        abstract protected PseudoStateVertex CreateS2PseudoState();
        abstract protected void AddBlankS2PseudoStateTransition();

        protected PseudoStateVertex s2_pseudoState;

        protected StateMachine sm;
        protected InitialState rootInitialState;
        protected State s1;
        protected State s2;
        protected State s2_1;
        protected State s2_1_1;

        public PseudoStateValidationTestHelper()
        {
            var sm = BuildTestGraph();
            inputSmBuilder.SetStateMachineRoot(sm);
        }

        private StateMachine BuildTestGraph()
        {
            sm = new StateMachine(name: "SomeSm");
            rootInitialState = sm.AddChild(new InitialState());
            s1 = sm.AddChild(new State(name: "s1"));
            s2 = sm.AddChild(new State(name: "s2"));
            s2_1 = s2.AddChild(new State(name: "s2_1"));
            s2_1_1 = s2_1.AddChild(new State(name: "s2_1_1"));
            rootInitialState.AddTransitionTo(s1);

            s2_pseudoState = s2.AddChild(CreateS2PseudoState());

            return sm;
        }

        [Fact]
        public void Children()
        {
            s2_pseudoState.AddChild(new State("s100"));
            ExpectVertexValidationException(exceptionMessagePart: "children");
        }

        [Fact]
        public void ParentNonNull()
        {
            s2_pseudoState._parent = null;
            ExpectVertexValidationException(exceptionMessagePart: "parent");
        }

        [Fact]
        public void NoDefaultTransition()
        {
            s2_pseudoState.Behaviors.Single().guardCode = "x > 100";
            ExpectVertexValidationExceptionWildcard("*default transition*");
        }

        [Fact]
        public virtual void GuardWithDefaultTransition()
        {
            s2_pseudoState.Behaviors.Single().guardCode = "x > 100";
            AddBlankS2PseudoStateTransition(); // default no-guard transition
            RunCompiler();
        }

        [Fact]
        public void TrueGuardOkAsDefaultTransition()
        {
            s2_pseudoState.Behaviors.Single().guardCode = "true";
            RunCompiler();
        }

        [Fact]
        public virtual void MultipleBehavior()
        {
            AddBlankS2PseudoStateTransition();
            RunCompiler();
        }

        [Fact]
        public void MustHaveTransition()
        {
            s2_pseudoState._behaviors.Clear();
            ExpectVertexValidationException(exceptionMessagePart: "must have at least one transition");
        }

        [Fact]
        public void Trigger()
        {
            PseudoStateVertex pseudoStateVertex = s2_pseudoState;
            s2_pseudoState.Behaviors[0]._triggers.Add("do");
            ExpectVertexValidationException(exceptionMessagePart: "trigger");
        }

        [Fact]
        public void IncomingTransitions()
        {
            s1.AddTransitionTo(s2_pseudoState);
            RunCompiler();
        }
    }

    public abstract class EntryInitialValidationTestHelper : PseudoStateValidationTestHelper
    {
        public EntryInitialValidationTestHelper()
        {
            s2_pseudoState.AddTransitionTo(s2_1);
        }

        protected override void AddBlankS2PseudoStateTransition()
        {
            s2_pseudoState.AddTransitionTo(s2_1);
        }

        [Fact]
        public void TargetOutsideOfParent()
        {
            s2_pseudoState.Behaviors.Single().RetargetTo(s1);
            ExpectVertexValidationException(exceptionMessagePart: "transition must remain within parent");
        }

        [Fact]
        public void TargetParent()
        {
            s2_pseudoState.Behaviors.Single().RetargetTo(s2_pseudoState.Parent);
            ExpectVertexValidationException(exceptionMessagePart: "cannot target parent");
        }
    }
}
