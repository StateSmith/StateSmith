#nullable enable

using StateSmith.Output;
using StateSmith.Runner;
using System.IO;
using System.Text;
using Xunit;

namespace StateSmithTest.Output.SmDescriberTest;

public class SmDesignDescriberTests
{
    [Fact]
    public void IntegrationTestDefault()
    {
        StringBuilder sb = Setup(SetupMode.Default);
        sb.ToString().ShouldBeShowDiff("");
    }

    [Fact]
    public void IntegrationTestDisabled()
    {
        StringBuilder sb = Setup(SetupMode.Disabled);
        sb.ToString().ShouldBeShowDiff("");
    }

    [Fact]
    public void IntegrationTestEnabled()
    {
        StringBuilder sb = Setup(SetupMode.Enabled);

        sb.ToString().ShouldBeShowDiff("""
            ##################################################
            # Before transformations
            ##################################################

            Vertex: <Notes>
            =================
            Type: NotesVertex
            Diagram Id: 140
            Notes:
                This is some stuff.
                
                Bold text.


            Vertex: <RenderConfig>
            =================
            Type: RenderConfigVertex
            Diagram Id: 141


            Vertex: <Config>(AutoExpandedVars)
            =================
            Parent: <RenderConfig>
            Type: ConfigOptionVertex
            Diagram Id: 145
            Option:
                int x;
                int y;


            Vertex: NORMAL
            =================
            Parent: ROOT
            Type: State
            Diagram Id: 138
            Behaviors:
                enter / { x = 0; }

                do / { x++; }

                exit / { x = 15; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT
            =================
            Type: StateMachine
            Diagram Id: 5
            Initial State: ROOT.<InitialState>
            Behaviors:
                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT.<InitialState>
            =================
            Parent: ROOT
            Type: InitialState
            Diagram Id: 8
            Behaviors:
                TransitionTo(DELAY)


            Vertex: DELAY
            =================
            Parent: NORMAL
            Type: State
            Diagram Id: 9
            Behaviors:
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
            =================
            Parent: ROOT
            Type: State
            Diagram Id: gO1ZRfetmGtumTaL4_i4-140
            Behaviors:
                PRESS / { buzz(); }

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING
            =================
            Parent: NORMAL
            Type: State
            Diagram Id: gO1ZRfetmGtumTaL4_i4-143
            Initial State: RUNNING.<InitialState>
            Behaviors:

                =========== from ancestor NORMAL ===========

                do / { x++; }

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: <RenderConfig>
            =================
            Parent: ROOT
            Type: RenderConfigVertex
            Diagram Id: gO1ZRfetmGtumTaL4_i4-146


            Vertex: <Config>(TriggerMap)
            =================
            Parent: <RenderConfig>
            Type: ConfigOptionVertex
            Diagram Id: gO1ZRfetmGtumTaL4_i4-148
            Option:
                // some comment
                ANY => * /* wildcard */


            Vertex: PRE_HEAT
            =================
            Parent: RUNNING
            Type: State
            Diagram Id: gO1ZRfetmGtumTaL4_i4-150
            Behaviors:
                enter / { set_timeout(5 min);
                x *= 23; }

                =========== from ancestor NORMAL ===========

                do / { x++; }

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING.<InitialState>
            =================
            Parent: RUNNING
            Type: InitialState
            Diagram Id: gO1ZRfetmGtumTaL4_i4-152
            Behaviors:
                TransitionTo(PRE_HEAT)





            ##################################################
            # After transformations
            ##################################################



            Vertex: NORMAL
            =================
            Parent: ROOT
            Type: State
            Diagram Id: 138
            Behaviors:
                enter / { x = 0; }

                do / { x++; }

                exit / { x = 15; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT
            =================
            Type: StateMachine
            Diagram Id: 5
            Initial State: ROOT.<InitialState>
            Behaviors:
                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT.<InitialState>
            =================
            Parent: ROOT
            Type: InitialState
            Diagram Id: 8
            Behaviors:
                TransitionTo(DELAY)


            Vertex: DELAY
            =================
            Parent: NORMAL
            Type: State
            Diagram Id: 9
            Behaviors:
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
            =================
            Parent: ROOT
            Type: State
            Diagram Id: gO1ZRfetmGtumTaL4_i4-140
            Behaviors:
                PRESS / { buzz(); }

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING
            =================
            Parent: NORMAL
            Type: State
            Diagram Id: gO1ZRfetmGtumTaL4_i4-143
            Initial State: RUNNING.<InitialState>
            Behaviors:

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: PRE_HEAT
            =================
            Parent: RUNNING
            Type: State
            Diagram Id: gO1ZRfetmGtumTaL4_i4-150
            Behaviors:
                enter / { set_timeout(5 min);
                x *= 23; }

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING.<InitialState>
            =================
            Parent: RUNNING
            Type: InitialState
            Diagram Id: gO1ZRfetmGtumTaL4_i4-152
            Behaviors:
                TransitionTo(PRE_HEAT)
            
            """);
    }

    private enum SetupMode { Enabled, Disabled, Default }

    private static StringBuilder Setup(SetupMode setupMode)
    {
        var sb = new StringBuilder();
        DiServiceProvider di;

        SmRunner smRunner = new(diagramPath: TestHelper.GetThisDir() + "Ex1.drawio");

        switch (setupMode)
        {
            case SetupMode.Enabled:
                smRunner.Settings.smDesignDescriber.enabled = true;
                break;
            case SetupMode.Disabled:
                smRunner.Settings.smDesignDescriber.enabled = false;
                break;
        }

        di = smRunner.GetExperimentalAccess().DiServiceProvider;
        di.AddSingletonT<ICodeGenRunner>(new DummyCodeGenRunner()); // to make test run faster

        var describer = di.GetInstanceOf<SmDesignDescriber>();
        describer.SetTextWriter(new StringWriter(sb));
        smRunner.Run();

        return sb;
    }
}
