#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.IO;
using System.Text;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmithTest.Output.SmDescriberTest;

public class SmDesignDescriberTests
{
    [Fact]
    public void IntegrationTestOffByDefault()
    {
        new Tester().RunAndExpect("");
    }

    [Fact]
    public void IntegrationTestDisabled()
    {
        new Tester().Disable().RunAndExpect("");
    }

    [Fact]
    public void IntegrationTestSmallEnabledDefault()
    {
        new Tester(diagramFile: "Ex565Tiny.drawio").Enable().RunAndExpect("""
            BEFORE TRANSFORMATIONS
            ===========================================================

            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                do / { x++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 7OOmFb1p9FNjOZE7ZJ_8-8

            ### Behaviors
                TransitionTo(S1)


            Vertex: S1
            -----------------------------------------
            - Parent: GROUP
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { a++; }

                [a > 10] / { blah(); } TransitionTo(S1)


            Vertex: S2
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-146

            ### Behaviors
                PRESS / { b++; }


            Vertex: GROUP
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-149

            ### Behaviors
                do / { b++; }

                PRESS [x > 10] TransitionTo(S2)

            """);
    }


    [Fact]
    public void IntegrationTestEnableAncestors()
    {
        new Tester(diagramFile: "Ex565Tiny.drawio").Enable().EnableOutputAncestorHandlers().RunAndExpect("""
            BEFORE TRANSFORMATIONS
            ===========================================================

            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                do / { x++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 7OOmFb1p9FNjOZE7ZJ_8-8

            ### Behaviors
                TransitionTo(S1)


            Vertex: S1
            -----------------------------------------
            - Parent: GROUP
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { a++; }

                [a > 10] / { blah(); } TransitionTo(S1)

                =========== from ancestor GROUP ===========

                do / { b++; }

                PRESS [x > 10] TransitionTo(S2)

                =========== from ancestor ROOT ===========

                do / { x++; }


            Vertex: S2
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-146

            ### Behaviors
                PRESS / { b++; }

                =========== from ancestor ROOT ===========

                do / { x++; }


            Vertex: GROUP
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-149

            ### Behaviors
                do / { b++; }

                PRESS [x > 10] TransitionTo(S2)

                =========== from ancestor ROOT ===========

                do / { x++; }

            """);
    }


    [Fact]
    public void IntegrationTestBeforeTransformationsOnly()
    {
        new Tester().Enable().DisableAfterTransformations().EnableOutputAncestorHandlers().RunAndExpect(ExpectedBeforeTransformations);
    }

    [Fact]
    public void IntegrationTestAfterTransformationsOnly()
    {
        new Tester().Enable().DisableBeforeTransformations().EnableAfterTransformations().EnableOutputAncestorHandlers().RunAndExpect(ExpectedAfterTransformations);
    }

    [Fact]
    public void IntegrationTestDisableBoth()
    {
        Action a = () => new Tester().DisableAfterTransformations().DisableBeforeTransformations().RunAndExpect(ExpectedAfterTransformations);
        a.Should().Throw<Exception>(); //TODO: better exception
    }

    [Fact]
    public void IntegrationTestToFile()
    {
        new Tester(captureToBuffer:false).Enable().EnableOutputAncestorHandlers().EnableAfterTransformations().Run();
    }

    /// <summary>
    /// 
    /// </summary>
    private class Tester
    {
        public StringBuilder sb = new();
        public SmRunner smRunner;
        public SmDesignDescriber describer;

        public Tester(bool captureToBuffer = true, string diagramFile = "Ex564.drawio")
        {
            smRunner = SetupSmRunner(out var iServiceProvider, diagramFile);
            describer = iServiceProvider.GetRequiredService<SmDesignDescriber>();
            if (captureToBuffer)
                describer.SetTextWriter(new StringWriter(sb));
        }

        public Tester Enable()
        {
            smRunner.Settings.smDesignDescriber.enabled = true;
            return this;
        }

        public Tester EnableOutputAncestorHandlers()
        {
            smRunner.Settings.smDesignDescriber.outputAncestorHandlers = true;
            return this;
        }

        public Tester Disable()
        {
            smRunner.Settings.smDesignDescriber.enabled = false;
            return this;
        }

        public Tester DisableBeforeTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.beforeTransformations = false;
            return this;
        }

        public Tester DisableAfterTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.afterTransformations = false;
            return this;
        }

        public Tester EnableAfterTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.afterTransformations = true;
            return this;
        }

        public void RunAndExpect(string expected)
        {
            Run();
            sb.ToString().ShouldBeShowDiff(expected, outputCleanActual:true);
        }

        public void Run()
        {
            smRunner.Run();
        }

        private static SmRunner SetupSmRunner(out IServiceProvider di, string diagramFile)
        {
            var spBuilder = IConfigServiceProviderBuilder.CreateDefault((services) =>
            {
                services.AddSingleton<ICodeGenRunner>(new DummyCodeGenRunner()); // to make test run faster
            });
            SmRunner smRunner = new(diagramPath: TestHelper.GetThisDir() + diagramFile, serviceProviderBuilder: spBuilder);
            smRunner.Settings.propagateExceptions = true; // for testing
            di = smRunner.GetExperimentalAccess().IServiceProvider;

            return smRunner;
        }
    }

    private const string ExpectedFull = $"""
            {ExpectedBeforeTransformations}


            <br>
            <br>
            <br>
            <br>
            <br>
            <br>

            {ExpectedAfterTransformations}
            """;


    private const string ExpectedBeforeTransformations = """
            BEFORE TRANSFORMATIONS
            ===========================================================

            Vertex: \<Notes>
            -----------------------------------------
            - Type: NotesVertex
            - Diagram Id: 140

            ### Notes Content:
                This is some stuff.
                
                Bold text.


            Vertex: \<RenderConfig>
            -----------------------------------------
            - Type: RenderConfigVertex
            - Diagram Id: 141


            Vertex: \<Config>(AutoExpandedVars)
            -----------------------------------------
            - Parent: \<RenderConfig>
            - Type: ConfigOptionVertex
            - Diagram Id: 145

            ### Option Content:
                int x;
                int y;


            Vertex: NORMAL
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: 138

            ### Behaviors
                enter / { x = 0; }

                do / { x++; }

                exit / { x = 15; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 8

            ### Behaviors
                TransitionTo(DELAY)


            Vertex: DELAY
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: 9

            ### Behaviors
                enter / { buzz(); }

                EV1 / { y = 33; }

                exit / { stop_buzz(); }

                [after_ms(100)
                && x >= 13] TransitionTo(RUNNING)

                =========== from ancestor NORMAL ===========

                do / { x++; }

                [err] TransitionTo(ERROR)
            
                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ERROR
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { buzz(); }

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-143
            - Initial State: RUNNING.\<InitialState>

            ### Behaviors

                =========== from ancestor NORMAL ===========

                do / { x++; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: PRE_HEAT
            -----------------------------------------
            - Parent: RUNNING
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-150

            ### Behaviors
                enter / { set_timeout(5 min);
                x *= 23; }

                =========== from ancestor NORMAL ===========

                do / { x++; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING.\<InitialState>
            -----------------------------------------
            - Parent: RUNNING
            - Type: InitialState
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-152

            ### Behaviors
                TransitionTo(PRE_HEAT)

            """;
            
            // TODO remove. I believe this is safe to remove because
            // it's only being used by SimWebGenerator, which does not need/expect
            // RenderConfig nodes to be output during the settings pass.

            // Vertex: \<RenderConfig>
            // -----------------------------------------
            // - Parent: ROOT
            // - Type: RenderConfigVertex
            // - Diagram Id: gO1ZRfetmGtumTaL4_i4-146


            // Vertex: \<Config>(TriggerMap)
            // -----------------------------------------
            // - Parent: \<RenderConfig>
            // - Type: ConfigOptionVertex
            // - Diagram Id: gO1ZRfetmGtumTaL4_i4-148

            // ### Option Content:
            //     // some comment
            //     ANY => * /* wildcard */


    private const string ExpectedAfterTransformations = """
            AFTER TRANSFORMATIONS
            ===========================================================

            Vertex: NORMAL
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: 138

            ### Behaviors
                enter / { x = 0; }

                do / { x++; }

                exit / { x = 15; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 8

            ### Behaviors
                TransitionTo(DELAY)


            Vertex: DELAY
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: 9

            ### Behaviors
                enter / { buzz(); }

                EV1 / { y = 33; }

                exit / { stop_buzz(); }

                do [after_ms(100)
                && x >= 13] TransitionTo(RUNNING)

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ERROR
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { buzz(); }

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-143
            - Initial State: RUNNING.\<InitialState>

            ### Behaviors

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: PRE_HEAT
            -----------------------------------------
            - Parent: RUNNING
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-150

            ### Behaviors
                enter / { set_timeout(5 min);
                x *= 23; }

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING.\<InitialState>
            -----------------------------------------
            - Parent: RUNNING
            - Type: InitialState
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-152

            ### Behaviors
                TransitionTo(PRE_HEAT)
            
            """;

}
