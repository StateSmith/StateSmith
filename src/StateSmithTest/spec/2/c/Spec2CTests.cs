using Spec.Spec2;
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
using System.Text.RegularExpressions;

namespace Spec.Spec2.C;

/// <summary>
/// Required so that we only do the gcc compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public SharedCompilationFixture()
    {
        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "gcc ../../lang-helpers/c/helper.c main.c Spec2Sm.c"
        };
        BashRunner.RunCommand(process);
    }

    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "c/";

    public class MyGlueFile : IRenderConfigC
    {
        Spec2GenericVarExpansions spec2GenericVarExpansions = new();

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
}

public class Spec2CFixture : IClassFixture<SharedCompilationFixture>
{
    public string Run(string initialEventToSelectTest, string testEvents)
    {
        SimpleProcess process = new()
        {
            WorkingDirectory = SharedCompilationFixture.OutputDirectory,
            CommandText = $"./a.out {initialEventToSelectTest} {testEvents}"
        };
        BashRunner.RunCommand(process);

        string output = process.StdOutput;
        output = StringUtils.RemoveEverythingBeforeRequiredMatch(output, "\nIGNORE_OUTPUT_BEFORE_THIS").Trim();

        return output;
    }

    public string PrepExpectedOutput(string expected)
    {
        expected = StringUtils.DeIndentTrim(expected);
        expected = StringUtils.ReplaceNewLineChars(expected, "\n");
        return expected;
    }
}

public class Spec2CTests : Spec2CFixture
{
    [Fact]
    public void TestDoEventHandling()
    {
        var output = Run(initialEventToSelectTest: "EV1", testEvents: "DO EV1 DO");

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        var expected = PrepExpectedOutput(@"
            Dispatch event DO
            ===================================================
            State TEST1_S1_1: check behavior `do`. Behavior running.
            State TEST1_ROOT: check behavior `do`. Behavior running.

            Dispatch event EV1
            ===================================================
            State TEST1_S1_1: check behavior `EV1 TransitionTo(TEST1_S2)`. Behavior running.
            Transition action `` for TEST1_S1_1 to TEST1_S2.
            Exit TEST1_S1_1.
            Exit TEST1_S1.
            Enter TEST1_S2.

            Dispatch event DO
            ===================================================
            State TEST1_S2: check behavior `do / { consume_event = true; }`. Behavior running.
        ");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void TestRegularEventHandling()
    {
        var output = Run(initialEventToSelectTest: "EV2", testEvents: "EV2 EV1 DO EV1");

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        var expected = PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST2_ROOT: check behavior `EV2`. Behavior running.

            Dispatch event EV1
            ===================================================
            State TEST2_S1_1: check behavior `EV1`. Behavior running.

            Dispatch event DO
            ===================================================
            State TEST2_S1_1: check behavior `do TransitionTo(TEST2_S2)`. Behavior running.
            Transition action `` for TEST2_S1_1 to TEST2_S2.
            Exit TEST2_S1_1.
            Exit TEST2_S1.
            Enter TEST2_S2.

            Dispatch event EV1
            ===================================================
            State TEST2_S2: check behavior `EV1 / { consume_event = false; }`. Behavior running.
            State TEST2_ROOT: check behavior `EV1`. Behavior running.
        ");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void TestBehaviorOrdering()
    {
        var output = Run(initialEventToSelectTest: "EV3", testEvents: "EV1 EV1");

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        var expected = PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST3_S1: check behavior `1. EV1 TransitionTo(TEST3_S2)`. Behavior running.
            Transition action `` for TEST3_S1 to TEST3_S2.
            Exit TEST3_S1.
            Enter TEST3_S2.

            Dispatch event EV1
            ===================================================
            State TEST3_S2: check behavior `1. EV1 / { trace(""1 woot!""); }`. Behavior running.
            1 woot!
            State TEST3_S2: check behavior `1.1. EV1 / { trace(""2 woot!""); }`. Behavior running.
            2 woot!
            State TEST3_S2: check behavior `2. EV1 / { trace(""3 woot!""); } TransitionTo(TEST3_S3)`. Behavior running.
            Transition action `trace(""3 woot!"");` for TEST3_S2 to TEST3_S3.
            3 woot!
            Exit TEST3_S2.
            Enter TEST3_S3.
        ");
        Assert.Equal(expected, output);
    }
}



