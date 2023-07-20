using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using static StateSmith.Runner.StandardSmTransformer;

namespace StateSmithTest.Output.SmDescriberTest;

public class SmDescriberTests
{
    [Fact]
    public void Test()
    {
        var sb = new StringBuilder();

        SmGraphDescriber smDescriber = new(new StringWriter(sb));

        InputSmBuilder runner = new();
        runner.ConvertDrawIoFileNodesToVertices(filepath: TestHelper.GetThisDir() + "Ex1.drawio");
        runner.FindSingleStateMachine();

        smDescriber.OutputHeader("Before transformations");

        // Sort by diagram ID to keep consistent
        var sortedRootVertices = runner.GetRootVertices().OrderBy(v => v.DiagramId).ToList();
        foreach (var v in sortedRootVertices)
        {
            // Skip any other state machines in diagram file.
            // Done this way so that we still print root vertices that are notes and render configs.
            if (v is StateMachine sm && sm != runner.GetStateMachine())
                continue;

            smDescriber.Describe(v);
        }

        runner.FinishRunning();

        smDescriber.WriteLine("\n\n\n\n");
        smDescriber.OutputHeader("After transformations");
        smDescriber.Describe(runner.GetStateMachine());

        //sb.ToString().Should().Be("");

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
}
