using Spec.Spec1;
using StateSmith.Input.Expansions;
using StateSmith.output;
using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;
using StateSmith.Runner;
using System;
using System.Diagnostics;
using Xunit;
using FluentAssertions;
using System.Runtime.InteropServices;
using StateSmithTest.Processes;

namespace Spec.Spec1.C;

public class Spec1CTests
{
    public static string OutputDirectory => Spec1Fixture.Spec1Directory + "c/";

    public class MyGlueFile : IRenderConfigC
    {
        Spec1GenericVarExpansions spec1GenericVarExpansions = new();

        string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                #include <stdint.h>
            ");

        string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""../../lang-helpers/c/helper.h""
            ");

        string IRenderConfigC.VariableDeclarations => StringUtils.DeIndentTrim(@"
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
            CommandText = "gcc ../../lang-helpers/c/helper.c main.c Spec1Sm.c"
        };
        BashRunner.RunCommand(process);

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "./a.out EV2 EV1 EV2 EV1 EV2"
        };
        BashRunner.RunCommand(process);

        // uncomment below line if you want to see the whole output
        // process.StdOutput.Should().Be("");

        var expected = StringUtils.DeIndentTrim(@"
            Start Statemachine
            ===================================================
            Enter Spec1Sm.
            Transition action `` for Spec1Sm.InitialState to S.
            Transition action `` for S.InitialState to S1.
            Transition action `` for S1.InitialState to S11.
            Enter S.
            Enter S1.
            Enter S11.

            Dispatch event EV2
            ===================================================

            Dispatch event EV1
            ===================================================
            State S11: check behavior `EV1 TransitionTo(S1.ExitPoint(1))`. Behavior running.
            Transition action `` for S11 to S1.ExitPoint(1).
            Transition action `` for S1 to T11.
            Transition action `` for T11.EntryPoint(1) to T111.
            Exit S11.
            Exit S1.
            Enter T1.
            Enter T11.
            Enter T111.

            Dispatch event EV2
            ===================================================
            State T11: check behavior `EV2 TransitionTo(S1)`. Behavior running.
            Transition action `` for T11 to S1.
            Transition action `` for S1.InitialState to S11.
            Exit T111.
            Exit T11.
            Exit T1.
            Enter S1.
            Enter S11.

            Dispatch event EV1
            ===================================================
            State S11: check behavior `EV1 TransitionTo(S1.ExitPoint(1))`. Behavior running.
            Transition action `` for S11 to S1.ExitPoint(1).
            Transition action `` for S1 to T11.
            Transition action `` for T11.EntryPoint(1) to T111.
            Exit S11.
            Exit S1.
            Enter T1.
            Enter T11.
            Enter T111.

            Dispatch event EV2
            ===================================================
            State T11: check behavior `EV2 TransitionTo(S1)`. Behavior running.
            Transition action `` for T11 to S1.
            Transition action `` for S1.InitialState to S11.
            Exit T111.
            Exit T11.
            Exit T1.
            Enter S1.
            Enter S11.
        ");

        Assert.Equal(expected, process.StdOutput.Trim());
    }
}



