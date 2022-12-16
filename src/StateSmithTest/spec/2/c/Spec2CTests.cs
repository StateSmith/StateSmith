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
        output = StringUtils.RemoveEverythingBefore(output, "\nIGNORE_OUTPUT_BEFORE_THIS\n").Trim();
        output = Regex.Replace(output, @"[\s\S]*\nCLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\n[\s\S]*?\n\n", "").Trim();

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

    //-------------------------------------------------------------------------------------

    [Fact]
    public void Test4_ParentChildTransitions()
    {
        const string InitialEventToSelectTest = "EV4 EV1";
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

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    [Fact]
    public void Test4_ParentChildTransitions_SelfTransition()
    {
        const string InitialEventToSelectTest = "EV4 EV1";

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

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    // https://github.com/StateSmith/StateSmith/issues/49
    [Fact]
    public void Test4_ParentChildTransitions_SelfTransitionWithInitialState()
    {
        const string InitialEventToSelectTest = "EV4 EV1";
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

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    //-------------------------------------------------------------------------------------

    [Fact]
    public void Test4B_LocalTransitionExample()
    {
        const string InitialEventToSelectTest = "EV4 EV2";
        var testEvents = "";
        var ex = "";

        // 
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4B_G: check behavior `EV1 TransitionTo(TEST4B_G_1)`. Behavior running.
            Transition action `` for TEST4B_G to TEST4B_G_1.
            Enter TEST4B_G_1.
        ") + "\n\n";

        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST4B_G_1: check behavior `EV2 TransitionTo(TEST4B_G)`. Behavior running.
            Exit TEST4B_G_1.
            Transition action `` for TEST4B_G_1 to TEST4B_G.
        ") + "\n\n";

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();
      
        // output.Should().Be(""); // uncomment line if you want to see the whole output
        Assert.Equal(ex, output);
    }

    [Fact]
    public void Test4C_LocalTransitionAliasExample()
    {
        const string InitialEventToSelectTest = "EV4 EV3";
        var testEvents = "";
        var ex = "";

        // 
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4C_G: check behavior `EV1 TransitionTo(TEST4C_G_1)`. Behavior running.
            Transition action `` for TEST4C_G to TEST4C_G_1.
            Enter TEST4C_G_1.
        ") + "\n\n";

        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST4C_G_1: check behavior `EV2 TransitionTo(TEST4C_G)`. Behavior running.
            Exit TEST4C_G_1.
            Transition action `` for TEST4C_G_1 to TEST4C_G.
        ") + "\n\n";

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();
      
        // output.Should().Be(""); // uncomment line if you want to see the whole output
        Assert.Equal(ex, output);
    }

    [Fact]
    public void Test4D_ExternalTransitionExample()
    {
        const string InitialEventToSelectTest = "EV4 EV4";
        var testEvents = "";
        var ex = "";

        // 
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST4D_G: check behavior `EV1 TransitionTo(TEST4D_EXTERNAL.ChoicePoint())`. Behavior running.
            Exit TEST4D_G.
            Transition action `` for TEST4D_G to TEST4D_EXTERNAL.ChoicePoint().
            Transition action `` for TEST4D_EXTERNAL.ChoicePoint() to TEST4D_G_1.
            Enter TEST4D_G.
            Enter TEST4D_G_1.
        ") + "\n\n";

        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@"
            Dispatch event EV2
            ===================================================
            State TEST4D_G_1: check behavior `EV2 TransitionTo(TEST4D_EXTERNAL.ChoicePoint())`. Behavior running.
            Exit TEST4D_G_1.
            Exit TEST4D_G.
            Transition action `` for TEST4D_G_1 to TEST4D_EXTERNAL.ChoicePoint().
            Transition action `` for TEST4D_EXTERNAL.ChoicePoint() to TEST4D_G.
            Enter TEST4D_G.
        ") + "\n\n";

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();
      
        // output.Should().Be(""); // uncomment line if you want to see the whole output
        Assert.Equal(ex, output);
    }

    //-------------------------------------------------------------------------------------

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

        const string InitialEventToSelectTest = "EV5";
        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    [Fact]
    public void Test6_Variables()
    {
        const string InitialEventToSelectTest = "EV6";

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

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    /////////////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test7_Choice_1_DirectToInitial()
    {
        int incCount = 1;
        var expectedState = "TEST7_G_S1";
        Test7_RunWithXIncrementEvents(expectedState, incCount, directToInitialState: true);
    }

    [Fact]
    public void Test7_Choice_1()
    {
        int incCount = 1;
        var expectedState = "TEST7_G_S1";
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test7_Choice_2()
    {
        int incCount = 2;
        var expectedState = "TEST7_G_S2";
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test7_Choice_3()
    {
        int incCount = 0;
        var expectedState = "TEST7_G_S3";
        Test7_RunWithXIncrementEvents(expectedState, incCount);

        incCount = 3;
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    private void Test7_RunWithXIncrementEvents(string expectedState, int incCount, bool directToInitialState = false)
    {
        const string InitialEventToSelectTest = "EV7";

        var testEvents = "";
        var ex = "";

        ex += PrepExpectedOutput(@"
            Transition action `` for TEST7_ROOT.InitialState to TEST7_S1.
            Enter TEST7_S1.
        ") + "\n\n";

        for (int i = 0; i < incCount; i++)
        {
            // 
            testEvents += "EV5 ";
            ex += PrepExpectedOutput(@"
            Dispatch event EV5
            ===================================================
            State TEST7_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            ") + "\n\n";
        }

        if (directToInitialState)
        {
            // 
            testEvents += "EV3 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV3
            ===================================================
            State TEST7_S1: check behavior `EV3 TransitionTo(TEST7_G.InitialState)`. Behavior running.
            Exit TEST7_S1.
            Transition action `` for TEST7_S1 to TEST7_G.InitialState.
            Enter TEST7_G.
            Transition action `` for TEST7_G.InitialState to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else
        {
            // 
            testEvents += "EV1 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV1
            ===================================================
            State TEST7_S1: check behavior `EV1 TransitionTo(TEST7_G)`. Behavior running.
            Exit TEST7_S1.
            Transition action `` for TEST7_S1 to TEST7_G.
            Enter TEST7_G.
            Transition action `` for TEST7_G.InitialState to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }


        // 
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@$"
            Dispatch event EV2
            ===================================================
            State TEST7_G: check behavior `EV2 TransitionTo(TEST7_ROOT.InitialState)`. Behavior running.
            Exit {expectedState}.
            Exit TEST7_G.
            Transition action `` for TEST7_G to TEST7_ROOT.InitialState.
            Transition action `` for TEST7_ROOT.InitialState to TEST7_S1.
            Enter TEST7_S1.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test8_Choice_1_Direct1()
    {
        int incCount = 1;
        var expectedState = "TEST8_G_S1";
        Test8_RunWithXIncrementEvents(expectedState, incCount, variation: 1);
    }

    [Fact]
    public void Test8_Choice_1_Direct2()
    {
        int incCount = 1;
        var expectedState = "TEST8_G_S1";
        Test8_RunWithXIncrementEvents(expectedState, incCount, variation: 2);
    }

    [Fact]
    public void Test8_Choice_1()
    {
        int incCount = 1;
        var expectedState = "TEST8_G_S1";
        Test8_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test8_Choice_2()
    {
        int incCount = 2;
        var expectedState = "TEST8_G_S2";
        Test8_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test8_Choice_3()
    {
        int incCount = 0;
        var expectedState = "TEST8_G_S3";
        Test8_RunWithXIncrementEvents(expectedState, incCount);

        incCount = 3;
        Test8_RunWithXIncrementEvents(expectedState, incCount);
    }

    private void Test8_RunWithXIncrementEvents(string expectedState, int incCount, int variation = 0)
    {
        const string InitialEventToSelectTest = "EV8";

        var testEvents = "";
        var ex = "";

        ex += PrepExpectedOutput(@"
            Transition action `` for TEST8_ROOT.EntryPoint(1) to TEST8_S1.
            Enter TEST8_S1.
        ") + "\n\n";

        for (int i = 0; i < incCount; i++)
        {
            // 
            testEvents += "EV5 ";
            ex += PrepExpectedOutput(@"
            Dispatch event EV5
            ===================================================
            State TEST8_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            ") + "\n\n";
        }

        if (variation == 0)
        {
            // 
            testEvents += "EV1 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV1
            ===================================================
            State TEST8_S1: check behavior `1. EV1 TransitionTo(TEST8_G.EntryPoint(1))`. Behavior running.
            Exit TEST8_S1.
            Transition action `` for TEST8_S1 to TEST8_G.EntryPoint(1).
            Enter TEST8_G.
            Transition action `` for TEST8_G.EntryPoint(1) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else if (variation == 1)
        {
            // 
            testEvents += "EV6 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV6
            ===================================================
            State TEST8_S1: check behavior `EV6 TransitionTo(TEST8_G.EntryPoint(3))`. Behavior running.
            Exit TEST8_S1.
            Transition action `` for TEST8_S1 to TEST8_G.EntryPoint(3).
            Enter TEST8_G.
            Transition action `count += 0;` for TEST8_G.EntryPoint(3) to TEST8_G.EntryPoint(1).
            Transition action `` for TEST8_G.EntryPoint(1) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else if (variation == 2)
        {
            // 
            testEvents += "EV3 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV3
            ===================================================
            State TEST8_S1: check behavior `EV3 TransitionTo(TEST8_G.EntryPoint(3))`. Behavior running.
            Exit TEST8_S1.
            Transition action `` for TEST8_S1 to TEST8_G.EntryPoint(3).
            Enter TEST8_G.
            Transition action `count += 0;` for TEST8_G.EntryPoint(3) to TEST8_G.EntryPoint(1).
            Transition action `` for TEST8_G.EntryPoint(1) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else
        {
            throw new Exception("unsupported variation " + variation);
        }

        // 
        testEvents += "EV2 ";
        ex += PrepExpectedOutput(@$"
            Dispatch event EV2
            ===================================================
            State TEST8_G: check behavior `EV2 TransitionTo(TEST8_ROOT.EntryPoint(1))`. Behavior running.
            Exit {expectedState}.
            Exit TEST8_G.
            Transition action `` for TEST8_G to TEST8_ROOT.EntryPoint(1).
            Transition action `` for TEST8_ROOT.EntryPoint(1) to TEST8_S1.
            Enter TEST8_S1.
            ") + "\n\n";

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }

    //---------------------------------------------------------------------------------------------------

    [Fact]
    public void Test9_Choice_1()
    {
        int incCount = 1;
        var expectedState = "TEST9_G_S1";
        Test9_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test9_Choice_2()
    {
        int incCount = 2;
        var expectedState = "TEST9_G_S2";
        Test9_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test9_Choice_3()
    {
        int incCount = 0;
        var expectedState = "TEST9_G_S3";
        Test9_RunWithXIncrementEvents(expectedState, incCount);

        incCount = 3;
        Test9_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test9_Choice_4()
    {
        int incCount = 4;
        var expectedState = "TEST9_G_S4";
        Test9_RunWithXIncrementEvents(expectedState, incCount);
    }

    private void Test9_RunWithXIncrementEvents(string expectedState, int incCount)
    {
        const string InitialEventToSelectTest = "EV9 EV1 ";

        var testEvents = "";
        var ex = "";

        for (int i = 0; i < incCount; i++)
        {
            // 
            testEvents += "EV5 ";
            ex += PrepExpectedOutput(@"
            Dispatch event EV5
            ===================================================
            State TEST9_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            ") + "\n\n";
        }

        // 
        testEvents += "EV1 ";
        ex += PrepExpectedOutput(@$"
        Dispatch event EV1
        ===================================================
        State TEST9_S1_1: check behavior `EV1 TransitionTo(TEST9_S1.ExitPoint(1))`. Behavior running.
        Exit TEST9_S1_1.
        Transition action `` for TEST9_S1_1 to TEST9_S1.ExitPoint(1).
        Exit TEST9_S1.
        Transition action `` for TEST9_S1.ExitPoint(1) to {expectedState}.
        Enter {expectedState}.
        ") + "\n\n";

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();


        Assert.Equal(ex, output);
    }

    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test9A_ExitPointTargetsParent()
    {
        var output = Run(initialEventToSelectTest: "EV9 EV2", testEvents: "EV1");
        //output.Should().Be(""); // uncomment line if you want to see the whole output.

        var expected = PrepExpectedOutput(@"
            Dispatch event EV1
            ===================================================
            State TEST9A_S1_1: check behavior `EV1 TransitionTo(TEST9A_S1.ExitPoint(1))`. Behavior running.
            Exit TEST9A_S1_1.
            State TEST9A_S1_1: check behavior `exit / { count = 100; }`. Behavior running.
            Transition action `` for TEST9A_S1_1 to TEST9A_S1.ExitPoint(1).
            Exit TEST9A_S1.
            Transition action `count++;` for TEST9A_S1.ExitPoint(1) to TEST9A_S1.
            Transition action `` for TEST9A_S1.InitialState to TEST9A_S1_1.
            Enter TEST9A_S1_1.
            State TEST9A_S1_1: check behavior `enter [count == 0] / { clear_output(); }`. Behavior skipped.
        ");

        // output.Should().Be("");  // uncomment line if you want to see the whole output.

        Assert.Equal(expected, output);
    }

    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test10_Choice_0()
    {
        int incCount = 0;
        var expectedState = "TEST10_G_S0";
        Test10_RunWithXIncrementEventsOverVariations(expectedState, incCount);
    }

    [Fact]
    public void Test10_Choice_1()
    {
        int incCount = 1;
        var expectedState = "TEST10_G_S1";
        Test10_RunWithXIncrementEventsOverVariations(expectedState, incCount);
    }

    [Fact]
    public void Test10_Choice_2()
    {
        int incCount = 2;
        var expectedState = "TEST10_G_S2";
        Test10_RunWithXIncrementEventsOverVariations(expectedState, incCount);
    }

    [Fact]
    public void Test10_Choice_3()
    {
        int incCount = 3;
        var expectedState = "TEST10_G_S3";
        Test10_RunWithXIncrementEventsOverVariations(expectedState, incCount);
    }

    [Fact]
    public void Test10_Choice_4()
    {
        int incCount = 4;
        var expectedState = "TEST10_S4";
        Test10_RunWithXIncrementEventsOverVariations(expectedState, incCount);
    }

    private void Test10_RunWithXIncrementEventsOverVariations(string expectedState, int incCount)
    {
        for (int i = 1; i <= 3; i++)
        {
            Test10_RunWithXIncrementEvents33(expectedState, incCount, variation: i);
        }
    }

    private void Test10_RunWithXIncrementEvents33(string expectedState, int incCount, int variation)
    {
        const string InitialEventToSelectTest = "EV10";

        var testEvents = "";
        var ex = "";

        for (int i = 0; i < incCount; i++)
        {
            // 
            testEvents += "EV5 ";
            ex += PrepExpectedOutput(@"
            Dispatch event EV5
            ===================================================
            State TEST10_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            ") + "\n\n";
        }

        if (variation == 1)
        {
            // 
            testEvents += "EV1 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV1
            ===================================================
            State TEST10_S1: check behavior `EV1 TransitionTo(TEST10_G.EntryPoint(1))`. Behavior running.
            Exit TEST10_S1.
            Transition action `` for TEST10_S1 to TEST10_G.EntryPoint(1).
            Enter TEST10_G.
            Transition action `` for TEST10_G.EntryPoint(1) to TEST10_G.ChoicePoint().
            Transition action `` for TEST10_G.ChoicePoint() to TEST10_G.ChoicePoint(1).
            ") + "\n";
        }
        else if (variation == 2)
        {
            // 
            testEvents += "EV2 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV2
            ===================================================
            State TEST10_S1: check behavior `EV2 TransitionTo(TEST10_G.ChoicePoint())`. Behavior running.
            Exit TEST10_S1.
            Transition action `` for TEST10_S1 to TEST10_G.ChoicePoint().
            Enter TEST10_G.
            Transition action `` for TEST10_G.ChoicePoint() to TEST10_G.ChoicePoint(1).
            ") + "\n";
        }
        else if (variation == 3)
        {
            // 
            testEvents += "EV3 ";
            ex += PrepExpectedOutput(@$"
            Dispatch event EV3
            ===================================================
            State TEST10_S1: check behavior `EV3 TransitionTo(TEST10_G)`. Behavior running.
            Exit TEST10_S1.
            Transition action `` for TEST10_S1 to TEST10_G.
            Enter TEST10_G.
            Transition action `` for TEST10_G.InitialState to TEST10_G.ChoicePoint().
            Transition action `` for TEST10_G.ChoicePoint() to TEST10_G.ChoicePoint(1).
            ") + "\n";
        }
        else
        {
            throw new Exception("unsupported variation " + variation);
        }

        if (incCount == 0)
        {
            ex += PrepExpectedOutput(@$"
            Transition action `` for TEST10_G.ChoicePoint(1) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else if (incCount == 1 || incCount == 2)
        {
            ex += PrepExpectedOutput(@$"
            Transition action `` for TEST10_G.ChoicePoint(1) to TEST10_G.ChoicePoint(lower).
            Transition action `` for TEST10_G.ChoicePoint(lower) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else if (incCount == 3)
        {
            ex += PrepExpectedOutput(@$"
            Transition action `` for TEST10_G.ChoicePoint(1) to TEST10_G.ChoicePoint(upper).
            Transition action `` for TEST10_G.ChoicePoint(upper) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else if (incCount == 4)
        {
            ex += PrepExpectedOutput(@$"
            Transition action `` for TEST10_G.ChoicePoint(1) to TEST10_G.ChoicePoint(upper).
            Exit TEST10_G.
            Transition action `` for TEST10_G.ChoicePoint(upper) to {expectedState}.
            Enter {expectedState}.
            ") + "\n\n";
        }
        else
        {
            throw new Exception("unsupported incCount " + incCount);
        }

        var output = Run(initialEventToSelectTest: InitialEventToSelectTest, testEvents: testEvents);

        ex = ex.Trim();

        // uncomment below line if you want to see the whole output
        //output.Should().Be("");

        Assert.Equal(ex, output);
    }
}



