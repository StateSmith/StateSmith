using StateSmith.Compiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using StateSmith.compiler;
using FluentAssertions;

namespace StateSmithTest
{
    public class YedHiddenEdgeTests
    {
        private readonly Compiler compiler;
        private readonly Vertex root;
        private readonly NamedVertex state_1;
        private readonly NamedVertex state_2;
        private readonly NamedVertex state_2_1;
        private readonly NamedVertex state_2_1_1;
        private readonly NamedVertex state_2_2;

        public YedHiddenEdgeTests()
        {
            compiler = ExamplesTestHelpers.SetupAndValidateCompilerForTestInputFile("yed-hidden-edges1.graphml");
            root = compiler.rootVertices.Single();
            state_1 = root.Descendant("STATE_1");
            state_2 = root.Descendant("STATE_2");
            state_2_1 = root.Descendant("STATE_2_1");
            state_2_1_1 = root.Descendant("STATE_2_1_1");
            state_2_2 = root.Descendant("STATE_2_2");
        }

        [Fact]
        public void NormallyFine()
        {
            compiler.Validate();
        }

        [Fact]
        public void ToSelfBad()
        {
            var b = state_1.AddTransitionTo(state_1);
            b.DiagramId = "MyId123";

            Action action = () => compiler.Validate();
            action.Should().Throw<BehaviorValidationException>().WithMessage("yEd hidden self-to-self edge detected*issues/29*").Where(e => e.behavior.DiagramId == "MyId123");
        }

        /// <summary>
        /// <see cref="Behavior.IsBlankTransition"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void AssertNonBlankValidates(NamedVertex source, NamedVertex target)
        {
            state_1.AddTransitionTo(state_1).triggers.Add("EV1");
            compiler.Validate();

            state_1.AddTransitionTo(state_1).guardCode += "some_guard";
            compiler.Validate();

            state_1.AddTransitionTo(state_1).actionCode += "some_action()";
            compiler.Validate();

            state_1.AddTransitionTo(state_1).order = 123;
            compiler.Validate();

            // don't test via entry/exit as those validations cover this
        }

        [Fact]
        public void ToSelfOk()
        {
            AssertNonBlankValidates(source: state_1, target: state_1);
        }


        [Fact]
        public void OutOfParentFine()
        {
            state_2_1_1.AddTransitionTo(state_1);
            compiler.Validate();
        }

        [Fact]
        public void ToParentBad()
        {
            var b = state_2_1_1.AddTransitionTo(state_2_1);
            b.DiagramId = "MyId456";

            Action action = () => compiler.Validate();
            action.Should().Throw<BehaviorValidationException>().WithMessage("yEd hidden edge to ancestor detected*issues/29*").Where(e => e.behavior.DiagramId == "MyId456");
        }

        [Fact]
        public void ToParentOk()
        {
            AssertNonBlankValidates(source: state_2_1_1, target: state_2_1);
        }

        [Fact]
        public void ToParentParentBad()
        {
            var b = state_2_1_1.AddTransitionTo(state_2);
            b.DiagramId = "MyId789";

            Action action = () => compiler.Validate();
            action.Should().Throw<BehaviorValidationException>().WithMessage("yEd hidden edge to ancestor detected*issues/29*").Where(e => e.behavior.DiagramId == "MyId789");
        }

        [Fact]
        public void ToParentParentOk()
        {
            AssertNonBlankValidates(source: state_2_1_1, target: state_2);
        }
    }
}
