#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.IO;
using System.Text;
using Xunit;

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
        new Tester().disable().RunAndExpect("");
    }

    [Fact]
    public void IntegrationTestEnabled()
    {
        new Tester().enable().RunAndExpect(ExpectedFull);
    }

    [Fact]
    public void IntegrationTestDisableBeforeTransformations()
    {
        new Tester().enable().disableBeforeTransformations().RunAndExpect(ExpectedAfterTransformations);
    }

    [Fact]
    public void IntegrationTestDisableAfterTransformations()
    {
        new Tester().enable().disableAfterTransformations().RunAndExpect(ExpectedBeforeTransformations);
    }

    [Fact]
    public void IntegrationTestDisableBoth()
    {
        Action a = () => new Tester().disableAfterTransformations().disableBeforeTransformations().RunAndExpect(ExpectedAfterTransformations);
        a.Should().Throw<Exception>(); //TODO: better exception
    }

    [Fact]
    public void IntegrationTestToFile()
    {
        var smRunner = SetupSmRunner();
        smRunner.Settings.smDesignDescriber.enabled = true;
        smRunner.Run();
    }

    private class Tester
    {
        public StringBuilder sb = new();
        public SmRunner smRunner;
        public SmDesignDescriber describer;

        public Tester()
        {
            smRunner = SetupSmRunner(out var diServiceProvider);
            describer = diServiceProvider.GetInstanceOf<SmDesignDescriber>();
            describer.SetTextWriter(new StringWriter(sb));
        }

        public Tester enable()
        {
            smRunner.Settings.smDesignDescriber.enabled = true;
            return this;
        }

        public Tester disable()
        {
            smRunner.Settings.smDesignDescriber.enabled = false;
            return this;
        }

        public Tester disableBeforeTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.beforeTransformations = false;
            return this;
        }

        public Tester disableAfterTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.afterTransformations = false;
            return this;
        }

        public void RunAndExpect(string expected)
        {
            smRunner.Run();
            sb.ToString().ShouldBeShowDiff(expected);
        }   
    }

    private static SmRunner SetupSmRunner()
    {
        return SetupSmRunner(out _);
    }


    private static SmRunner SetupSmRunner(out DiServiceProvider di)
    {
        SmRunner smRunner = new(diagramPath: TestHelper.GetThisDir() + "Ex564.drawio");
        smRunner.Settings.propagateExceptions = true; // for testing
        di = smRunner.GetExperimentalAccess().DiServiceProvider;
        di.AddSingletonT<ICodeGenRunner>(new DummyCodeGenRunner()); // to make test run faster

        return smRunner;
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

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: \<RenderConfig>
            -----------------------------------------
            - Parent: ROOT
            - Type: RenderConfigVertex
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-146


            Vertex: \<Config>(TriggerMap)
            -----------------------------------------
            - Parent: \<RenderConfig>
            - Type: ConfigOptionVertex
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-148

            ### Option Content:
                // some comment
                ANY => * /* wildcard */


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
