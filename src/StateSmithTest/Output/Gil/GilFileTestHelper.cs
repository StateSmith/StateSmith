using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;

namespace StateSmithTest.Output.Gil;

public class GilFileTestHelper
{
    // Used by other tests
    public static string BuildExampleGilFile(bool skipIndentation, out StateMachine sm)
    {
        sm = new("TestsMySm1");
        var s1 = sm.AddChild(new State("S1"));
        var s2 = sm.AddChild(new State("S2"));

        sm.AddChild(new InitialState()).AddTransitionTo(s1).actionCode = "this.vars.b = false;";
        s1.AddTransitionTo(s2);

        sm.variables += "bool b;";

        void SetupAction(DiServiceProvider sp)
        {
            sp.AddSingletonT(new RenderConfigBaseVars()
            {
                VariableDeclarations = "//This is super cool!\nx: 0,"
            });

            sp.AddSingletonT(new AlgoBalanced1Settings()
            {
                skipClassIndentation = skipIndentation,
            });
        }

        InputSmBuilder inputSmBuilder = new(SetupAction);
        inputSmBuilder.SetStateMachineRoot(sm);
        inputSmBuilder.FinishRunning();

        //NameMangler mangler = new();
        //mangler.SetStateMachine(sm);
        //StateMachineProvider stateMachineProvider = new(sm);
        //EnumBuilder enumBuilder = new(mangler, stateMachineProvider);
        //PseudoStateHandlerBuilder pseudoStateHandlerBuilder = new();
        //EventHandlerBuilder eventHandlerBuilder = new(new(), pseudoStateHandlerBuilder, mangler);

        AlgoBalanced1 builder = inputSmBuilder.sp.GetInstanceOf<AlgoBalanced1>();
        return builder.GenerateGil(sm);
    }
}

