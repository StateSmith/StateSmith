using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Output.Gil;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StateSmithTest.Output.Gil;

public class MethodPtrFinderTests
{
    const string input = """
                
        // Generated state machine
        public class Spec2SmSLDFKJSLDFJSLDFJ
        {
            // event handler type
            private delegate void Func();

            // Used internally by state machine. Don't modify.
            private Func? ancestorEventHandler;

            // Used internally by state machine. Don't modify.
            private readonly Func?[] currentEventHandlers = new Func[5];

            // Used internally by state machine. Don't modify.
            private Func? currentStateExitHandler;


            // Dispatches an event to the state machine. Not thread safe.
            public void DispatchEvent(int i)
            {
                Func? behaviorFunc = this.currentEventHandlers[i];

                while (behaviorFunc != null)
                {
                    this.ancestorEventHandler = null;
                    behaviorFunc();
                    behaviorFunc = this.ancestorEventHandler;
                }
            }

            // This function is used when StateSmith doesn't know what the active leaf state is at
            // compile time due to sub states or when multiple states need to be exited.
            private void ExitUpToStateHandler(Func desiredStateExitHandler)
            {
                while (this.currentStateExitHandler != desiredStateExitHandler)
                {
                    this.currentStateExitHandler!();
                }
            }


            ////////////////////////////////////////////////////////////////////////////////
            // event handlers for state ROOT
            ////////////////////////////////////////////////////////////////////////////////

            private void ROOT_enter()
            {
                // setup trigger/event handlers
                this.currentStateExitHandler = ROOT_exit;
            }

            private void ROOT_exit()
            {

            }


            ////////////////////////////////////////////////////////////////////////////////
            // event handlers for state AUTO_VAR_TEST
            ////////////////////////////////////////////////////////////////////////////////

            private void AUTO_VAR_TEST_enter()
            {
                // setup trigger/event handlers
                this.currentStateExitHandler = AUTO_VAR_TEST_exit;
            }

            private void AUTO_VAR_TEST_exit()
            {
                // adjust function pointers for this state's exit
                this.currentStateExitHandler = ROOT_exit;
            }


            ////////////////////////////////////////////////////////////////////////////////
            // event handlers for state AUTO_VAR_TEST__BLAH
            ////////////////////////////////////////////////////////////////////////////////

            private void AUTO_VAR_TEST__BLAH_enter()
            {
                // setup trigger/event handlers
                //this.currentStateExitHandler = AUTO_VAR_TEST__BLAH_exit; // commented out so we can detect with ExitUpToStateHandler() below
                this.currentEventHandlers[1] = this.AUTO_VAR_TEST__BLAH_do;
            }

            private void AUTO_VAR_TEST__BLAH_exit()
            {
                // adjust function pointers for this state's exit
                this.currentStateExitHandler = AUTO_VAR_TEST_exit;
                this.currentEventHandlers[1] = null;  // no ancestor listens to this event
            }

            private void AUTO_VAR_TEST__BLAH_do()
            {
                ExitUpToStateHandler(this.AUTO_VAR_TEST__BLAH_exit);
            }
        }
        """;

    [Fact]
    public void Test1()
    {
        new RoslynCompiler().Compile(input, out CompilationUnitSyntax root, out SemanticModel model);

        var finder = new MethodPtrFinder(root, model);

        finder.Find();
        List<MethodDeclarationSyntax> found = finder.methods.ToList();

        List<string> expected = found.Select(mds => mds.Identifier.Text).ToList();

        expected.Should().BeEquivalentTo(new List<string>
        {
            "ROOT_exit",
            "AUTO_VAR_TEST_exit",
            "AUTO_VAR_TEST__BLAH_exit",
            "AUTO_VAR_TEST__BLAH_do",
        });

        finder.delegateDeclarationSyntax.Identifier.Text.Should().Be("Func");
        finder.delegateSymbol.Name.Should().Be("Func");
    }
}
