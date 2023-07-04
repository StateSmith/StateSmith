using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;
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

        SmDescriber smDescriber = new(new StringWriter(sb));

        InputSmBuilder runner = new();
        runner.ConvertDrawIoFileNodesToVertices(filepath: TestHelper.GetThisDir() + "Ex1.drawio");
        runner.FindSingleStateMachine();

        runner.transformer.InsertAfterFirstMatch(TransformationId.Standard_FinalValidation, new TransformationStep("", (StateMachine sm) =>
        {
            smDescriber.Describe(sm);
        }));

        runner.FinishRunning();

        sb.ToString().ShouldBeShowDiff("""
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
                enter / { set_timeout(5 min); }

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
