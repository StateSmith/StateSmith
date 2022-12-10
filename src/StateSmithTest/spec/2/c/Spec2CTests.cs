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
    public void Test1_DoEventHandling()
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
            Exit TEST1_S1_1.
            Exit TEST1_S1.
            Transition action `` for TEST1_S1_1 to TEST1_S2.
            Enter TEST1_S2.

            Dispatch event DO
            ===================================================
            State TEST1_S2: check behavior `do / { consume_event = true; }`. Behavior running.
        ");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Test1_DoEventHandling_v2()
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
            Exit TEST1_S1_1.
            Exit TEST1_S1.
            Transition action `` for TEST1_S1_1 to TEST1_S2.
            Enter TEST1_S2.

            Dispatch event DO
            ===================================================
            State TEST1_S2: check behavior `do / { consume_event = true; }`. Behavior running.
        ");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Test2_RegularEventHandling()
    {
        var output = Run(initialEventToSelectTest: "EV2", testEvents: "EV2 EV1 DO EV1 EV2");

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
            Exit TEST2_S1_1.
            Exit TEST2_S1.
            Transition action `` for TEST2_S1_1 to TEST2_S2.
            Enter TEST2_S2.

            Dispatch event EV1
            ===================================================
            State TEST2_S2: check behavior `EV1 / { consume_event = false; }`. Behavior running.
            State TEST2_ROOT: check behavior `EV1`. Behavior running.

            Dispatch event EV2
            ===================================================
            State TEST2_S2: check behavior `EV2 TransitionTo(TEST2_S2)`. Behavior running.
            Exit TEST2_S2.
            Transition action `` for TEST2_S2 to TEST2_S2.
            Enter TEST2_S2.
        ");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Test3_BehaviorOrdering()
    {
        var output = Run(initialEventToSelectTest: "EV3", testEvents: "EV1 EV1");

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        var expected = PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST3_S1: check behavior `1. EV1 TransitionTo(TEST3_S2)`. Behavior running.
            Exit TEST3_S1.
            Transition action `` for TEST3_S1 to TEST3_S2.
            Enter TEST3_S2.

            Dispatch event EV1
            ===================================================
            State TEST3_S2: check behavior `1. EV1 / { trace(""1 woot!""); }`. Behavior running.
            1 woot!
            State TEST3_S2: check behavior `1.1. EV1 / { trace(""2 woot!""); }`. Behavior running.
            2 woot!
            State TEST3_S2: check behavior `2. EV1 / { trace(""3 woot!""); } TransitionTo(TEST3_S3)`. Behavior running.
            Exit TEST3_S2.
            Transition action `trace(""3 woot!"");` for TEST3_S2 to TEST3_S3.
            3 woot!
            Enter TEST3_S3.
        ");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Test4_ParentChildTransitions()
    {
        var testEvents = "";
        var ex = "";

        // should see transition to S1 without exiting ROOT
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST4_ROOT: check behavior `EV2 TransitionTo(TEST4_S1)`. Behavior running.
            Transition action `` for TEST4_ROOT to TEST4_S1.
            Enter TEST4_S1.
            ") + "\n\n";


        // Already in S1. Root handler should exit S1, then re-enter S1.
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST4_ROOT: check behavior `EV2 TransitionTo(TEST4_S1)`. Behavior running.
            Exit TEST4_S1.
            Transition action `` for TEST4_ROOT to TEST4_S1.
            Enter TEST4_S1.
            ") + "\n\n";

        // Should transition from S1 to S2
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4_S1: check behavior `EV1 TransitionTo(TEST4_S2)`. Behavior running.
            Exit TEST4_S1.
            Transition action `` for TEST4_S1 to TEST4_S2.
            Enter TEST4_S2.
            ") + "\n\n";

        // Root handler should cause transition to S1.
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST4_ROOT: check behavior `EV2 TransitionTo(TEST4_S1)`. Behavior running.
            Exit TEST4_S2.
            Transition action `` for TEST4_ROOT to TEST4_S1.
            Enter TEST4_S1.
            ") + "\n\n";

        // S1 to S2
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4_S1: check behavior `EV1 TransitionTo(TEST4_S2)`. Behavior running.
            Exit TEST4_S1.
            Transition action `` for TEST4_S1 to TEST4_S2.
            Enter TEST4_S2.
            ") + "\n\n";

        // S2 to S3
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4_S2: check behavior `EV1 TransitionTo(TEST4_S3)`. Behavior running.
            Exit TEST4_S2.
            Transition action `` for TEST4_S2 to TEST4_S3.
            Enter TEST4_S3.
            ") + "\n\n";

        // S3 to ROOT
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4_S3: check behavior `EV1 TransitionTo(TEST4_ROOT)`. Behavior running.
            Exit TEST4_S3.
            Transition action `` for TEST4_S3 to TEST4_ROOT.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: "EV4", testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    [Fact]
    public void Test4_ParentChildTransitions_SelfTransition()
    {
        var testEvents = "";
        var ex = "";

        // 
        testEvents += "EV3 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV3
            ===================================================
            State TEST4_ROOT: check behavior `EV3 TransitionTo(TEST4_S10_1)`. Behavior running.
            Transition action `` for TEST4_ROOT to TEST4_S10_1.
            Enter TEST4_S10.
            Enter TEST4_S10_1.
            ") + "\n\n";

        testEvents += "EV4 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV4
            ===================================================
            State TEST4_S10: check behavior `EV4 TransitionTo(TEST4_S10)`. Behavior running.
            Exit TEST4_S10_1.
            Exit TEST4_S10.
            Transition action `` for TEST4_S10 to TEST4_S10.
            Enter TEST4_S10.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: "EV4", testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    // https://github.com/StateSmith/StateSmith/issues/49
    [Fact]
    public void Test4_ParentChildTransitions_SelfTransitionWithInitialState()
    {
        var testEvents = "";
        var ex = "";

        // 
        testEvents += "EV4 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV4
            ===================================================
            State TEST4_ROOT: check behavior `EV4 TransitionTo(TEST4_S20)`. Behavior running.
            Transition action `` for TEST4_ROOT to TEST4_S20.
            Enter TEST4_S20.
            Transition action `` for TEST4_S20.InitialState to TEST4_S20_1.
            Enter TEST4_S20_1.
            ") + "\n\n";

        testEvents += "EV4 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV4
            ===================================================
            State TEST4_S20: check behavior `EV4 TransitionTo(TEST4_S20)`. Behavior running.
            Exit TEST4_S20_1.
            Exit TEST4_S20.
            Transition action `` for TEST4_S20 to TEST4_S20.
            Enter TEST4_S20.
            Transition action `` for TEST4_S20.InitialState to TEST4_S20_1.
            Enter TEST4_S20_1.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: "EV4", testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    /// <summary>
    /// Same as <see cref="TestParentChildTransitions"/>, but designed with parent alias nodes instead.
    /// </summary>
    [Fact]
    public void Test5_ParentAliasChildTransitions()
    {
        var testEvents = "";
        var ex = "";

        // should see transition to S1 without exiting ROOT
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST5_ROOT: check behavior `EV2 TransitionTo(TEST5_S1)`. Behavior running.
            Transition action `` for TEST5_ROOT to TEST5_S1.
            Enter TEST5_S1.
            ") + "\n\n";


        // Already in S1. Root handler should exit S1, then re-enter S1.
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST5_ROOT: check behavior `EV2 TransitionTo(TEST5_S1)`. Behavior running.
            Exit TEST5_S1.
            Transition action `` for TEST5_ROOT to TEST5_S1.
            Enter TEST5_S1.
            ") + "\n\n";

        // Should transition from S1 to S2
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST5_S1: check behavior `EV1 TransitionTo(TEST5_S2)`. Behavior running.
            Exit TEST5_S1.
            Transition action `` for TEST5_S1 to TEST5_S2.
            Enter TEST5_S2.
            ") + "\n\n";

        // Root handler should cause transition to S1.
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST5_ROOT: check behavior `EV2 TransitionTo(TEST5_S1)`. Behavior running.
            Exit TEST5_S2.
            Transition action `` for TEST5_ROOT to TEST5_S1.
            Enter TEST5_S1.
            ") + "\n\n";

        // S1 to S2
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST5_S1: check behavior `EV1 TransitionTo(TEST5_S2)`. Behavior running.
            Exit TEST5_S1.
            Transition action `` for TEST5_S1 to TEST5_S2.
            Enter TEST5_S2.
            ") + "\n\n";

        // S2 to S3
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST5_S2: check behavior `EV1 TransitionTo(TEST5_S3)`. Behavior running.
            Exit TEST5_S2.
            Transition action `` for TEST5_S2 to TEST5_S3.
            Enter TEST5_S3.
            ") + "\n\n";

        // S3 to ROOT
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST5_S3: check behavior `EV1 TransitionTo(TEST5_ROOT)`. Behavior running.
            Exit TEST5_S3.
            Transition action `` for TEST5_S3 to TEST5_ROOT.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: "EV5", testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    [Fact]
    public void Test6_Variables()
    {
        var testEvents = "";
        var ex = "";

        // 
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST6_S1: check behavior `1. EV1 / { count++; }`. Behavior running.
            State TEST6_S1: check behavior `2. EV1 [count >= 2] TransitionTo(TEST6_S2)`. Behavior skipped.
            ") + "\n\n";

        // 
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST6_S1: check behavior `1. EV1 / { count++; }`. Behavior running.
            State TEST6_S1: check behavior `2. EV1 [count >= 2] TransitionTo(TEST6_S2)`. Behavior running.
            Exit TEST6_S1.
            Transition action `` for TEST6_S1 to TEST6_S2.
            Enter TEST6_S2.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: "EV6", testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }
}



