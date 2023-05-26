using StateSmith.Output;
using StateSmith.Output.UserConfig;
using Xunit;
using StateSmithTest.Processes;

namespace Spec.Spec1.C;

public class Spec1CTests
{
    public static string OutputDirectory => Spec1Fixture.Spec1Directory + "c/";

    public class MyGlueFile : IRenderConfig, IRenderConfigC
    {
        string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ");

        string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""../../lang-helpers/c/helper.h""
            ");

        string IRenderConfig.VariableDeclarations => StringUtils.DeIndentTrim(@"
                uint8_t count;
            ");
    }

    [Fact]
    public void CodeGenAndTest()
    {
        Spec1Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "gcc -Wall ../../lang-helpers/c/helper.c main.c Spec1Sm.c"
        };
        BashRunner.RunCommand(process);

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "./a.out EV2 EV1 EV2 "
        };
        BashRunner.RunCommand(process);

        // uncomment below line if you want to see the whole output
        // process.StdOutput.Should().Be("");

        var expected = StringUtils.DeIndentTrim(@"
            Start Statemachine
            ===================================================
            Enter Spec1Sm.
            Transition action `` for ROOT.<InitialState> to S.
            Enter S.
            Transition action `` for S.<InitialState> to S1.
            Enter S1.
            Transition action `` for S1.<InitialState> to S11.
            Enter S11.

            Dispatch event EV2
            ===================================================

            Dispatch event EV1
            ===================================================
            State S11: check behavior `EV1 TransitionTo(S1.<ExitPoint>(1))`. Behavior running.
            Exit S11.
            Transition action `` for S11 to S1.<ExitPoint>(1).
            Exit S1.
            Transition action `` for S1.<ExitPoint>(1) to T11.<EntryPoint>(1).
            Enter T1.
            Enter T11.
            Transition action `` for T11.<EntryPoint>(1) to T111.
            Enter T111.

            Dispatch event EV2
            ===================================================
            State T11: check behavior `EV2 TransitionTo(S1)`. Behavior running.
            Exit T111.
            Exit T11.
            Exit T1.
            Transition action `` for T11 to S1.
            Enter S1.
            Transition action `` for S1.<InitialState> to S11.
            Enter S11.
        ");

        Assert.Equal(expected, process.StdOutput.Trim());
    }
}



