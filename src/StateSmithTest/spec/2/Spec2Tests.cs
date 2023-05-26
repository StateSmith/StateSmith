using Spec;
using Spec.Spec2;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace StateSmithTest.spec2;

public abstract class Spec2Tests : Spec2Fixture, IDisposable
{
    SpecTester tester = new();
    public const string GilStart = Test7History1Helper.GilStart;
    public const string GilEnd = Test7History1Helper.GilEnd;

    public abstract string RunProcess(string testEvents);

    public void RunTester()
    {
        if (tester.HasExpectations())
        {
            tester.RunAndVerify(RunProcess);
        }
    }

    // Ensures that tester is run after being setup
    public void Dispose()
    {
        RunTester();
    }

    public string PrepExpectedOutput(string expected)
    {
        return SpecTester.PrepExpectedOutput(expected);
    }

    public string PreProcessOutput(string output)
    {
        output = Regex.Replace(output, @"\w+__([a-zA-Z]+)", "$1");
        return output;
    }

    [Fact]
    public void Test1_DoEventHandling()
    {
        tester.PreEvents = "EV1";

        tester.AddEventHandling("DO", t => t(@"
            State TEST1_S1_1: check behavior `do`. Behavior running.
            State TEST1_ROOT: check behavior `do`. Behavior running.
            State Spec2Sm: check behavior `do`. Behavior running.
        ")); tester.AddEventHandling("EV1", t => t(@"
            State TEST1_S1_1: check behavior `EV1 TransitionTo(TEST1_S2)`. Behavior running.
            Exit TEST1_S1_1.
            Exit TEST1_S1.
            Transition action `` for TEST1_S1_1 to TEST1_S2.
            Enter TEST1_S2.
        ")); tester.AddEventHandling("DO", t => t(@"
            State TEST1_S2: check behavior `do / { consume_event = true; }`. Behavior running.
        "));
    }



    [Fact]
    public void Test2_RegularEventHandling()
    {
        tester.PreEvents = "EV2";

        tester.AddEventHandling("EV2", t => t(@"
            State TEST2_ROOT: check behavior `EV2`. Behavior running.
        ")); tester.AddEventHandling("EV1", t => t(@"
            State TEST2_S1_1: check behavior `EV1`. Behavior running.
        ")); tester.AddEventHandling("DO", t => t(@"
            State TEST2_S1_1: check behavior `do TransitionTo(TEST2_S2)`. Behavior running.
            Exit TEST2_S1_1.
            Exit TEST2_S1.
            Transition action `` for TEST2_S1_1 to TEST2_S2.
            Enter TEST2_S2.
            State TEST2_S2: check behavior `en`. Behavior running.
        ")); tester.AddEventHandling("EV1", t => t(@"
            State TEST2_S2: check behavior `ev1 / { consume_event = false; }`. Behavior running.
            State TEST2_ROOT: check behavior `EV1`. Behavior running.
        ")); tester.AddEventHandling("EV2", t => t(@"
            State TEST2_S2: check behavior `ev2 TransitionTo(TEST2_S2)`. Behavior running.
            Exit TEST2_S2.
            Transition action `` for TEST2_S2 to TEST2_S2.
            Enter TEST2_S2.
            State TEST2_S2: check behavior `en`. Behavior running.
        "));
    }

    [Fact]
    public void Test3_BehaviorOrdering()
    {
        tester.PreEvents = "EV3";

        tester.AddEventHandling("EV1", t => t(@"
            State TEST3_S1: check behavior `1. EV1 TransitionTo(TEST3_S2)`. Behavior running.
            Exit TEST3_S1.
            Transition action `` for TEST3_S1 to TEST3_S2.
            Enter TEST3_S2.
        ")); tester.AddEventHandling("EV1", t => t(@"
            State TEST3_S2: check behavior `1. EV1 / { trace(""1 woot!""); }`. Behavior running.
            1 woot!
            State TEST3_S2: check behavior `1.1. EV1 / { trace(""2 woot!""); }`. Behavior running.
            2 woot!
            State TEST3_S2: check behavior `2. EV1 / { trace(""3 woot!""); } TransitionTo(TEST3_S3)`. Behavior running.
            Exit TEST3_S2.
            Transition action `trace(""3 woot!"");` for TEST3_S2 to TEST3_S3.
            3 woot!
            Enter TEST3_S3.
        "));
    }

    //-------------------------------------------------------------------------------------

    [Fact]
    public void Test4_ParentChildTransitions()
    {
        tester.PreEvents = "EV4 EV1";

        // should see transition to S1 without exiting ROOT
        tester.AddEventHandling("EV2", t => t(@"
            State TEST4_ROOT: check behavior `EV2 TransitionTo(TEST4_S1)`. Behavior running.
            Transition action `` for TEST4_ROOT to TEST4_S1.
            Enter TEST4_S1.
        "));

        // Already in S1. Root handler should exit S1, then re-enter S1.
        tester.AddEventHandling("EV2", t => t(@"
            State TEST4_ROOT: check behavior `EV2 TransitionTo(TEST4_S1)`. Behavior running.
            Exit TEST4_S1.
            Transition action `` for TEST4_ROOT to TEST4_S1.
            Enter TEST4_S1.
        "));

        // Should transition from S1 to S2
        tester.AddEventHandling("EV1", t => t(@"
            State TEST4_S1: check behavior `EV1 TransitionTo(TEST4_S2)`. Behavior running.
            Exit TEST4_S1.
            Transition action `` for TEST4_S1 to TEST4_S2.
            Enter TEST4_S2.
        "));

        // Root handler should cause transition to S1.
        tester.AddEventHandling("EV2", t => t(@"
            State TEST4_ROOT: check behavior `EV2 TransitionTo(TEST4_S1)`. Behavior running.
            Exit TEST4_S2.
            Transition action `` for TEST4_ROOT to TEST4_S1.
            Enter TEST4_S1.
        "));

        // S1 to S2
        tester.AddEventHandling("EV1", t => t(@"
            State TEST4_S1: check behavior `EV1 TransitionTo(TEST4_S2)`. Behavior running.
            Exit TEST4_S1.
            Transition action `` for TEST4_S1 to TEST4_S2.
            Enter TEST4_S2.
        "));

        // S2 to S3
        tester.AddEventHandling("EV1", t => t(@"
            State TEST4_S2: check behavior `EV1 TransitionTo(TEST4_S3)`. Behavior running.
            Exit TEST4_S2.
            Transition action `` for TEST4_S2 to TEST4_S3.
            Enter TEST4_S3.
        "));

        // S3 to ROOT
        tester.AddEventHandling("EV1", t => t(@"
            State TEST4_S3: check behavior `EV1 TransitionTo(TEST4_ROOT)`. Behavior running.
            Exit TEST4_S3.
            Transition action `` for TEST4_S3 to TEST4_ROOT.
        "));
    }

    [Fact]
    public void Test4_ParentChildTransitions_SelfTransition()
    {
        tester.PreEvents = "EV4 EV1";

        // 
        tester.AddEventHandling("EV3", t => t(@"
            State TEST4_ROOT: check behavior `EV3 TransitionTo(TEST4_S10_1)`. Behavior running.
            Transition action `` for TEST4_ROOT to TEST4_S10_1.
            Enter TEST4_S10.
            Enter TEST4_S10_1.
        "));

        tester.AddEventHandling("EV4", t => t(@"
            State TEST4_S10: check behavior `EV4 TransitionTo(TEST4_S10)`. Behavior running.
            Exit TEST4_S10_1.
            Exit TEST4_S10.
            Transition action `` for TEST4_S10 to TEST4_S10.
            Enter TEST4_S10.
        "));
    }

    // https://github.com/StateSmith/StateSmith/issues/49
    [Fact]
    public void Test4_ParentChildTransitions_SelfTransitionWithInitialState()
    {
        tester.PreEvents = "EV4 EV1";

        // 
        tester.AddEventHandling("EV4", t => t(@"
            State TEST4_ROOT: check behavior `EV4 TransitionTo(TEST4_S20)`. Behavior running.
            Transition action `` for TEST4_ROOT to TEST4_S20.
            Enter TEST4_S20.
            Transition action `` for TEST4_S20.<InitialState> to TEST4_S20_1.
            Enter TEST4_S20_1.
        "));

        tester.AddEventHandling("EV4", t => t(@"
            State TEST4_S20: check behavior `EV4 TransitionTo(TEST4_S20)`. Behavior running.
            Exit TEST4_S20_1.
            Exit TEST4_S20.
            Transition action `` for TEST4_S20 to TEST4_S20.
            Enter TEST4_S20.
            Transition action `` for TEST4_S20.<InitialState> to TEST4_S20_1.
            Enter TEST4_S20_1.
        "));
    }

    //-------------------------------------------------------------------------------------

    [Fact]
    public void Test4B_LocalTransitionExample()
    {
        tester.PreEvents = "EV4 EV2";

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State TEST4B_G: check behavior `EV1 TransitionTo(TEST4B_G_1)`. Behavior running.
            Transition action `` for TEST4B_G to TEST4B_G_1.
            Enter TEST4B_G_1.
        "));

        tester.AddEventHandling("EV2", t => t(@"
            State TEST4B_G_1: check behavior `EV2 TransitionTo(TEST4B_G)`. Behavior running.
            Exit TEST4B_G_1.
            Transition action `` for TEST4B_G_1 to TEST4B_G.
        "));
    }

    [Fact]
    public void Test4C_LocalTransitionAliasExample()
    {
        tester.PreEvents = "EV4 EV3";

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State TEST4C_G: check behavior `EV1 TransitionTo(TEST4C_G_1)`. Behavior running.
            Transition action `` for TEST4C_G to TEST4C_G_1.
            Enter TEST4C_G_1.
        "));

        tester.AddEventHandling("EV2", t => t(@"
            State TEST4C_G_1: check behavior `EV2 TransitionTo(TEST4C_G)`. Behavior running.
            Exit TEST4C_G_1.
            Transition action `` for TEST4C_G_1 to TEST4C_G.
        "));
    }

    [Fact]
    public void Test4D_ExternalTransitionExample()
    {
        tester.PreEvents = "EV4 EV4";

        tester.AddEventHandling("EV1", t => t(@"
            State TEST4D_G: check behavior `EV1 TransitionTo(TEST4D_EXTERNAL.<ChoicePoint>())`. Behavior running.
            Exit TEST4D_G.
            Transition action `` for TEST4D_G to TEST4D_EXTERNAL.<ChoicePoint>().
            Transition action `` for TEST4D_EXTERNAL.<ChoicePoint>() to TEST4D_G_1.
            Enter TEST4D_G.
            Enter TEST4D_G_1.
        "));

        tester.AddEventHandling("EV2", t => t(@"
            State TEST4D_G_1: check behavior `EV2 TransitionTo(TEST4D_EXTERNAL.<ChoicePoint>())`. Behavior running.
            Exit TEST4D_G_1.
            Exit TEST4D_G.
            Transition action `` for TEST4D_G_1 to TEST4D_EXTERNAL.<ChoicePoint>().
            Transition action `` for TEST4D_EXTERNAL.<ChoicePoint>() to TEST4D_G.
            Enter TEST4D_G.
        "));
    }

    //-------------------------------------------------------------------------------------

    /// <summary>
    /// Same as <see cref="TestParentChildTransitions"/>, but designed with parent alias nodes instead.
    /// </summary>
    [Fact]
    public void Test5_ParentAliasChildTransitions()
    {
        tester.PreEvents = "EV5";

        // should see transition to S1 without exiting ROOT
        tester.AddEventHandling("EV2", t => t(@"
            State TEST5_ROOT: check behavior `EV2 TransitionTo(TEST5_S1)`. Behavior running.
            Transition action `` for TEST5_ROOT to TEST5_S1.
            Enter TEST5_S1.
        "));

        // Already in S1. Root handler should exit S1, then re-enter S1.
        tester.AddEventHandling("EV2", t => t(@"
            State TEST5_ROOT: check behavior `EV2 TransitionTo(TEST5_S1)`. Behavior running.
            Exit TEST5_S1.
            Transition action `` for TEST5_ROOT to TEST5_S1.
            Enter TEST5_S1.
        "));

        // Should transition from S1 to S2
        tester.AddEventHandling("EV1", t => t(@"
            State TEST5_S1: check behavior `EV1 TransitionTo(TEST5_S2)`. Behavior running.
            Exit TEST5_S1.
            Transition action `` for TEST5_S1 to TEST5_S2.
            Enter TEST5_S2.
        "));

        // Root handler should cause transition to S1.
        tester.AddEventHandling("EV2", t => t(@"
            State TEST5_ROOT: check behavior `EV2 TransitionTo(TEST5_S1)`. Behavior running.
            Exit TEST5_S2.
            Transition action `` for TEST5_ROOT to TEST5_S1.
            Enter TEST5_S1.
        "));

        // S1 to S2
        tester.AddEventHandling("EV1", t => t(@"
            State TEST5_S1: check behavior `EV1 TransitionTo(TEST5_S2)`. Behavior running.
            Exit TEST5_S1.
            Transition action `` for TEST5_S1 to TEST5_S2.
            Enter TEST5_S2.
        "));

        // S2 to S3
        tester.AddEventHandling("EV1", t => t(@"
            State TEST5_S2: check behavior `EV1 TransitionTo(TEST5_S3)`. Behavior running.
            Exit TEST5_S2.
            Transition action `` for TEST5_S2 to TEST5_S3.
            Enter TEST5_S3.
        "));

        // S3 to ROOT
        tester.AddEventHandling("EV1", t => t(@"
            State TEST5_S3: check behavior `EV1 TransitionTo(TEST5_ROOT)`. Behavior running.
            Exit TEST5_S3.
            Transition action `` for TEST5_S3 to TEST5_ROOT.
        "));
    }

    [Fact]
    public void Test6_Variables_Normal()
    {
        tester.PreEvents = "EV6 EV1";

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S1: check behavior `1. EV1 / { count++; }`. Behavior running.
            State S1: check behavior `2. EV1 [count >= 2] TransitionTo(S2)`. Behavior skipped.
        "));

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S1: check behavior `1. EV1 / { count++; }`. Behavior running.
            State S1: check behavior `2. EV1 [count >= 2] TransitionTo(S2)`. Behavior running.
            Exit S1.
            Transition action `` for S1 to S2.
            Enter S2.
        "));
    }

    [Fact]
    public void Test6_Variables_AutoVar()
    {
        tester.PreEvents = "EV6 EV2";

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S1: check behavior `1. EV1 / { auto_var_1++; }`. Behavior running.
            State S1: check behavior `2. EV1 [auto_var_1 == 2] TransitionTo(S2)`. Behavior skipped.
        "));

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S1: check behavior `1. EV1 / { auto_var_1++; }`. Behavior running.
            State S1: check behavior `2. EV1 [auto_var_1 == 2] TransitionTo(S2)`. Behavior running.
            Exit S1.
            Transition action `` for S1 to S2.
            Enter S2.
        "));
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/45
    /// </summary>
    [Fact]
    public void Test6_MetaExpansions()
    {
        tester.PreEvents = "EV6 EV3";

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S1: check behavior `EV1 / { trace_meta(); } TransitionTo(S2)`. Behavior running.
            Exit S1.
            Transition action `trace_meta();` for S1 to S2.
            META: State: S1, trigger: ev1, behavior vertex: S1
            Enter S2.
            State S2: check behavior `enter / { trace_meta(); }`. Behavior running.
            META: State: S2, trigger: enter, behavior vertex: S2
        "));

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S2: check behavior `EV1 / { trace_meta(); } TransitionTo(META_EXPANSIONS.<ChoicePoint>(1))`. Behavior running.
            Exit S2.
            Transition action `trace_meta();` for S2 to META_EXPANSIONS.<ChoicePoint>(1).
            META: State: S2, trigger: ev1, behavior vertex: S2
            Transition action `trace_meta();` for META_EXPANSIONS.<ChoicePoint>(1) to S3.
            META: State: S2, trigger: ev1, behavior vertex: META_EXPANSIONS.<ChoicePoint>(1)
            Enter S3.
        "));

        // 
        tester.AddEventHandling("EV1", t => t(@"
            State S3: check behavior `EV1 / { trace_meta(); } TransitionTo(META_EXPANSIONS.<ChoicePoint>(2))`. Behavior running.
            Exit S3.
            Transition action `trace_meta();` for S3 to META_EXPANSIONS.<ChoicePoint>(2).
            META: State: S3, trigger: ev1, behavior vertex: S3
            Transition action `trace_meta();` for META_EXPANSIONS.<ChoicePoint>(2) to S5.
            META: State: META_EXPANSIONS, trigger: , behavior vertex: META_EXPANSIONS.<ChoicePoint>(2)
            Enter S5.
        "));
    }

    /////////////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test7_Choice_1_DirectToInitial()
    {
        int incCount = 1;
        var expectedState = "G_S1";
        Test7_RunWithXIncrementEvents(expectedState, incCount, directToInitialState: true);
    }

    [Fact]
    public void Test7_Choice_1()
    {
        int incCount = 1;
        var expectedState = "G_S1";
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test7_Choice_2()
    {
        int incCount = 2;
        var expectedState = "G_S2";
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test7_Choice_3()
    {
        int incCount = 0;
        var expectedState = "G_S3";
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    [Fact]
    public void Test7_Choice_3_2()
    {
        int incCount = 3;
        var expectedState = "G_S3";
        Test7_RunWithXIncrementEvents(expectedState, incCount);
    }

    private void Test7_RunWithXIncrementEvents(string expectedState, int incCount, bool directToInitialState = false)
    {
        tester.PreEvents = "EV7 EV1";

        for (int i = 0; i < incCount; i++)
        {
            // 
            tester.AddEventHandling("EV5", t => t(@"
                State PARENT: check behavior `EV5 / { count++; }`. Behavior running.
            "));
        }

        if (directToInitialState)
        {
            // 
            tester.AddEventHandling("EV3", t => t(@$"
                State S1: check behavior `EV3 TransitionTo(G.<InitialState>)`. Behavior running.
                Exit S1.
                Transition action `` for S1 to G.<InitialState>.
                Enter G.
                Transition action `` for G.<InitialState> to {expectedState}.
                Enter {expectedState}.
            "));
        }
        else
        {
            // 
            tester.AddEventHandling("EV1", t => t(@$"
                State S1: check behavior `EV1 TransitionTo(G)`. Behavior running.
                Exit S1.
                Transition action `` for S1 to G.
                Enter G.
                Transition action `` for G.<InitialState> to {expectedState}.
                Enter {expectedState}.
            "));
        }

        // 
        tester.AddEventHandling("EV2", t => t(@$"
            State G: check behavior `EV2 TransitionTo(PARENT.<InitialState>)`. Behavior running.
            Exit {expectedState}.
            Exit G.
            Transition action `` for G to PARENT.<InitialState>.
            Transition action `` for PARENT.<InitialState> to S1.
            Enter S1.
        "));
    }

    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test7_History1_BackAndForth()
    {
        Test7History1Helper helper = new(tester);
        helper.StartToStateOff2();

        // back to on states
        tester.AddEventHandling(helper.EventOffToOn, t => t($@"
            State OFF: check behavior `EV7 TransitionTo(ON)`. Behavior running.
            Exit OFF2.
            Exit OFF.
            Transition action `` for OFF to ON.
            Enter ON.
            Transition action `` for ON.<InitialState> to ON.<History>.
            Transition action `` for ON.<History> to ON2.
            Enter ON2.
            State ON2: check behavior `enter / {{ {GilStart}{helper.OnVarName} = {helper.OnEnumAccess}ON2;{GilEnd} }}`. Behavior running.
        "));

        // over to off states
        tester.AddEventHandling(helper.EventOnToOff, t => t($@"
            State ON: check behavior `EV6 TransitionTo(OFF)`. Behavior running.
            Exit ON2.
            Exit ON.
            Transition action `` for ON to OFF.
            Enter OFF.
            Transition action `` for OFF.<InitialState> to OFF.<History>.
            Transition action `` for OFF.<History> to OFF2.
            Enter OFF2.
            State OFF2: check behavior `enter / {{ {GilStart}{helper.OffVarName} = {helper.OffEnumAccess}OFF2;{GilEnd} }}`. Behavior running.
        "));
    }

    [Fact]
    public void Test7_History1_OffToOff3()
    {
        Test7History1Helper helper = new(tester);
        helper.StartToStateOff1();

        tester.AddEventHandling("EV3", t => t($@"
            State OFF: check behavior `EV3 TransitionTo(OFF3)`. Behavior running.
            Exit OFF1.
            Transition action `` for OFF to OFF3.
            Enter OFF3.
            State OFF3: check behavior `enter / {{ {GilStart}{helper.OffVarName} = {helper.OffEnumAccess}OFF3;{GilEnd} }}`. Behavior running.
        "));
    }

    [Fact]
    public void Test7_History1_OffSelfTranstion_1()
    {
        Test7History1Helper helper = new(tester);
        helper.StartToStateOff1();

        tester.AddEventHandling("EV4", t => t($@"
            State OFF: check behavior `EV4 TransitionTo(OFF)`. Behavior running.
            Exit OFF1.
            Exit OFF.
            Transition action `` for OFF to OFF.
            Enter OFF.
            Transition action `` for OFF.<InitialState> to OFF.<History>.
            Transition action `` for OFF.<History> to OFF1.
            Enter OFF1.
            State OFF1: check behavior `enter / {{ {GilStart}{helper.OffVarName} = {helper.OffEnumAccess}OFF1;{GilEnd} }}`. Behavior running.
        "));
    }

    [Fact]
    public void Test7_History1_OffSelfTranstion_2()
    {
        Test7History1Helper helper = new(tester);
        helper.StartToStateOff2();

        tester.AddEventHandling("EV4", t => t($@"
            State OFF: check behavior `EV4 TransitionTo(OFF)`. Behavior running.
            Exit OFF2.
            Exit OFF.
            Transition action `` for OFF to OFF.
            Enter OFF.
            Transition action `` for OFF.<InitialState> to OFF.<History>.
            Transition action `` for OFF.<History> to OFF2.
            Enter OFF2.
            State OFF2: check behavior `enter / {{ {GilStart}{helper.OffVarName} = {helper.OffEnumAccess}OFF2;{GilEnd} }}`. Behavior running.
        "));
    }

    [Fact]
    public void Test7_History1_OffSelfTranstion_3()
    {
        Test7History1Helper helper = new(tester);
        helper.StartToStateOff3();

        tester.AddEventHandling("EV4", t => t($@"
            State OFF: check behavior `EV4 TransitionTo(OFF)`. Behavior running.
            Exit OFF3.
            Exit OFF.
            Transition action `` for OFF to OFF.
            Enter OFF.
            Transition action `` for OFF.<InitialState> to OFF.<History>.
            Transition action `` for OFF.<History> to OFF3.
            Enter OFF3.
            State OFF3: check behavior `enter / {{ {GilStart}{helper.OffVarName} = {helper.OffEnumAccess}OFF3;{GilEnd} }}`. Behavior running.
        "));
    }

    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test7_DeepHistory1_1()
    {
        Test7DeepHistory1Helper helper = new(tester);
        helper.StartToToyWallE();

        tester.AddEventHandling(helper.EventAlienDetected, t => t($@"
             {helper.ExpectBuildToAliensExiting("WALL_E", "ROBOT", "TOY")}
             Transition action `` for ALIENS_DETECTED.<History> to SNOWBALL_FIGHT.
             Enter SNOWBALL_FIGHT.
             State SNOWBALL_FIGHT: check behavior `enter / {{ {GilStart}{helper.AliensVarName} = {helper.AliensEnumAccess}SNOWBALL_FIGHT;{GilEnd} }}`. Behavior running.
         "));

        helper.SnowballFight_to_GiveCookies();

        tester.AddEventHandling(helper.EventAlienDone, t => t($@"
            State ALIENS_DETECTED: check behavior `EV8 TransitionTo(BUILD)`. Behavior running.
            Exit GIVE_COOKIES.
            Exit ALIENS_DETECTED.
            Transition action `` for ALIENS_DETECTED to BUILD.
            Enter BUILD.
            Transition action `` for BUILD.<InitialState> to BUILD.<History>.
            Transition action `` for BUILD.<History> to WALL_E.
            Enter TOY.
            State TOY: check behavior `enter / {{ {GilStart}{helper.BuildVarName} = {helper.BuildEnumAccess}TOY;{GilEnd} }}`. Behavior running.
            Enter ROBOT.
            State ROBOT: check behavior `enter / {{ {GilStart}{helper.BuildVarName} = {helper.BuildEnumAccess}ROBOT;{GilEnd} }}`. Behavior running.
            Enter WALL_E.
            State WALL_E: check behavior `enter / {{ {GilStart}{helper.BuildVarName} = {helper.BuildEnumAccess}WALL_E;{GilEnd} }}`. Behavior running.
        "));

        tester.AddEventHandling(helper.EventAlienDetected, t => t($@"
             {helper.ExpectBuildToAliensExiting("WALL_E", "ROBOT", "TOY")}
             Transition action `` for ALIENS_DETECTED.<History> to GIVE_COOKIES.
             {helper.EnterGiveCookiesText()}
         "));
    }


    [Fact]
    public void Test7_DeepHistory1_2()
    {
        Test7DeepHistory1Helper helper = new(tester);
        helper.StartToTool_ImpactDrill();

        tester.AddEventHandling(helper.EventAlienDetected, t => t($@"
             {helper.ExpectBuildToAliensExiting("IMPACT_DRILL", "TOOL")}
             Transition action `` for ALIENS_DETECTED.<History> to SNOWBALL_FIGHT.
             Enter SNOWBALL_FIGHT.
             State SNOWBALL_FIGHT: check behavior `enter / {{ {GilStart}{helper.AliensVarName} = {helper.AliensEnumAccess}SNOWBALL_FIGHT;{GilEnd} }}`. Behavior running.
         "));

        helper.SnowballFight_to_GiveCookies();
        helper.GiveCookies_to_CallThor();
        helper.CallThor_to_CallBatman();

        tester.AddEventHandling(helper.EventAlienDone, t => t($@"
            State ALIENS_DETECTED: check behavior `EV8 TransitionTo(BUILD)`. Behavior running.
            Exit CALL_BATMAN.
            Exit HERO.
            Exit GET_BACKUP.
            Exit ALIENS_DETECTED.
            Transition action `` for ALIENS_DETECTED to BUILD.
            Enter BUILD.
            Transition action `` for BUILD.<InitialState> to BUILD.<History>.
            Transition action `` for BUILD.<History> to IMPACT_DRILL.
            {helper.EnterToolText()}
            {helper.EnterImpactDrillText()}
        "));

        helper.ImpactDrill_to_CircularSaw();

        // was in CALL_BATMAN last, but it isn't saved in history. HERO state is though.
        tester.AddEventHandling(helper.EventAlienDetected, t => t($@"
             {helper.ExpectBuildToAliensExiting("CIRCULAR_SAW", "TOOL")}
             Transition action `` for ALIENS_DETECTED.<History> to HERO.
             {helper.EnterGetBackupText()}
             {helper.EnterHeroText()}
             Transition action `` for HERO.<InitialState> to CALL_THOR.
             Enter CALL_THOR.
         "));

        helper.CallThor_to_CallBatman();
        helper.CallBatman_to_BuddyElf();

        tester.AddEventHandling(helper.EventAlienDone, t => t($@"
            State ALIENS_DETECTED: check behavior `EV8 TransitionTo(BUILD)`. Behavior running.
            Exit BUDDY_ELF.
            Exit LOCAL_HELP.
            Exit GET_BACKUP.
            Exit ALIENS_DETECTED.
            Transition action `` for ALIENS_DETECTED to BUILD.
            Enter BUILD.
            Transition action `` for BUILD.<InitialState> to BUILD.<History>.
            Transition action `` for BUILD.<History> to CIRCULAR_SAW.
            {helper.EnterToolText()}
            {helper.EnterCircularSawText()}
        "));

        tester.AddEventHandling(helper.EventAlienDetected, t => t($@"
             {helper.ExpectBuildToAliensExiting("CIRCULAR_SAW", "TOOL")}
             Transition action `` for ALIENS_DETECTED.<History> to BUDDY_ELF.
             {helper.EnterGetBackupText()}
             {helper.EnterLocalHelpText()}
             {helper.EnterBuddyELfText()}
         "));

        tester.AddEventHandling(helper.EventAlienDone, t => t($@"
            State ALIENS_DETECTED: check behavior `EV8 TransitionTo(BUILD)`. Behavior running.
            Exit BUDDY_ELF.
            Exit LOCAL_HELP.
            Exit GET_BACKUP.
            Exit ALIENS_DETECTED.
            Transition action `` for ALIENS_DETECTED to BUILD.
            Enter BUILD.
            Transition action `` for BUILD.<InitialState> to BUILD.<History>.
            Transition action `` for BUILD.<History> to CIRCULAR_SAW.
            {helper.EnterToolText()}
            {helper.EnterCircularSawText()}
        "));

        tester.AddEventHandling(helper.EventAlienDetectedWantHero, t => t($@"
            State BUILD: check behavior `EV7 TransitionTo(GET_BACKUP.<History>)`. Behavior running.
            Exit CIRCULAR_SAW.
            Exit TOOL.
            Exit BUILD.
            Transition action `` for BUILD to GET_BACKUP.<History>.
            Enter ALIENS_DETECTED.
            {helper.EnterGetBackupText()}
            Transition action `` for GET_BACKUP.<History> to BUDDY_ELF.
            {helper.EnterLocalHelpText()}
            {helper.EnterBuddyELfText()}
         "));
    }

    [Fact]
    public void Test7_DeepHistory1_HistoryDefaultToChoice()
    {
        Test7DeepHistory1Helper helper = new(tester);
        helper.StartToToyRaceCar();

        // go directly to GET_BACKUP.<History> without it having stored a value so that we see it
        // use its default transition to a choice state
        tester.AddEventHandling(helper.EventAlienDetectedWantHero, t => t($@"
            State BUILD: check behavior `EV7 TransitionTo(GET_BACKUP.<History>)`. Behavior running.
            Exit RACE_CAR.
            Exit TOY.
            Exit BUILD.
            Transition action `` for BUILD to GET_BACKUP.<History>.
            Enter ALIENS_DETECTED.
            {helper.EnterGetBackupText()}
            Transition action `` for GET_BACKUP.<History> to GET_BACKUP.<ChoicePoint>().
            Transition action `` for GET_BACKUP.<ChoicePoint>() to HERO.
            {helper.EnterHeroText()}
            Transition action `` for HERO.<InitialState> to CALL_THOR.
            Enter CALL_THOR.
         "));
    }


    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test7_DeepHistory2()
    {
        tester.PreEvents = "EV7 EV4";
        var historyVar = Test7History1Helper.SmVars + ".state_0_history";
        var historyEnumAccess = "state_0_HistoryId.";

        // go from state_1 to state_6
        tester.AddEventHandling("evStep", t => t($$"""
            State state_1: check behavior `evStep TransitionTo(state_2)`. Behavior running.
            Exit state_1.
            Transition action `` for state_1 to state_2.
            Enter state_2.
            State state_2: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_2;{{GilEnd}} }`. Behavior running.
            Transition action `` for state_2.<InitialState> to state_6.
            Enter state_6.
            State state_6: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_6;{{GilEnd}} }`. Behavior running.
            """
         ));

        // get to state 9
        tester.AddEventHandling("evStep", t => t($$"""
            State state_6: check behavior `evStep TransitionTo(state_9)`. Behavior running.
            Exit state_6.
            Transition action `` for state_6 to state_9.
            Enter state_9.
            State state_9: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_9;{{GilEnd}} }`. Behavior running.
            """
        ));

        // exit all the way out to state 3
        tester.AddEventHandling("evOpen", t => t($$"""
            State state_0: check behavior `evOpen TransitionTo(state_3)`. Behavior running.
            Exit state_9.
            Exit state_2.
            Exit state_0.
            Transition action `` for state_0 to state_3.
            Enter state_3.
            """
        ));

        // re-enter and verify history functionality
        tester.AddEventHandling("evClose", t => t($$"""
            State state_3: check behavior `evClose TransitionTo(state_0)`. Behavior running.
            Exit state_3.
            Transition action `` for state_3 to state_0.
            Enter state_0.
            Transition action `` for state_0.<InitialState> to state_0.<History>.
            Transition action `` for state_0.<History> to state_9.
            Enter state_2.
            State state_2: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_2;{{GilEnd}} }`. Behavior running.
            Enter state_9.
            State state_9: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_9;{{GilEnd}} }`. Behavior running.
            """
        ));

        // uncomment below to see full output
        // RunTester();
        // Assert.True(false, tester.Output);
    }

    [Fact]
    public void Test7_DeepHistory3()
    {
        tester.PreEvents = "EV7 EV5";
        var historyVar = Test7History1Helper.SmVars + ".state_0_history";
        var historyEnumAccess = "state_0_HistoryId.";

        // go from state_1 to state_6
        tester.AddEventHandling("evStep", t => t($$"""
            State state_1: check behavior `evStep TransitionTo(state_2)`. Behavior running.
            Exit state_1.
            Transition action `` for state_1 to state_2.
            Enter state_2.
            State state_2: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_2;{{GilEnd}} }`. Behavior running.
            Transition action `` for state_2.<InitialState> to state_6.
            Enter state_6.
            """
         ));

        // get to state 9
        tester.AddEventHandling("evStep", t => t($$"""
            State state_6: check behavior `evStep TransitionTo(state_9)`. Behavior running.
            Exit state_6.
            Transition action `` for state_6 to state_9.
            Enter state_9.
            """
        ));

        // exit all the way out to state 3
        tester.AddEventHandling("evOpen", t => t($$"""
            State state_0: check behavior `evOpen TransitionTo(state_3)`. Behavior running.
            Exit state_9.
            Exit state_2.
            Exit state_0.
            Transition action `` for state_0 to state_3.
            Enter state_3.
            """
        ));

        // re-enter and verify history functionality
        tester.AddEventHandling("evClose", t => t($$"""
            State state_3: check behavior `evClose TransitionTo(state_0)`. Behavior running.
            Exit state_3.
            Transition action `` for state_3 to state_0.
            Enter state_0.
            Transition action `` for state_0.<InitialState> to state_0.<History>.
            Transition action `` for state_0.<History> to state_2.
            Enter state_2.
            State state_2: check behavior `enter / { {{GilStart}}{{historyVar}} = {{historyEnumAccess}}STATE_2;{{GilEnd}} }`. Behavior running.
            Transition action `` for state_2.<InitialState> to state_6.
            Enter state_6.
            """
        ));

        // uncomment below to see full output
        // RunTester(); Assert.True(false, tester.Output);
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
    }

    [Fact]
    public void Test8_Choice_3_2()
    {
        int incCount = 3;
        var expectedState = "TEST8_G_S3";
        Test8_RunWithXIncrementEvents(expectedState, incCount);
    }

    private void Test8_RunWithXIncrementEvents(string expectedState, int incCount, int variation = 0)
    {
        tester.PreEvents = "EV8";

        for (int i = 0; i < incCount; i++)
        {
            // 
            tester.AddEventHandling("EV5", t => t(@"
                State TEST8_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            "));
        }

        if (variation == 0)
        {
            // 
            tester.AddEventHandling("EV1", t => t(@$"
                State TEST8_S1: check behavior `1. EV1 TransitionTo(TEST8_G.<EntryPoint>(1))`. Behavior running.
                Exit TEST8_S1.
                Transition action `` for TEST8_S1 to TEST8_G.<EntryPoint>(1).
                Enter TEST8_G.
                Transition action `` for TEST8_G.<EntryPoint>(1) to {expectedState}.
                Enter {expectedState}.
            "));
        }
        else if (variation == 1)
        {
            // 
            tester.AddEventHandling("EV6", t => t(@$"
                State TEST8_S1: check behavior `EV6 TransitionTo(TEST8_G.<EntryPoint>(3))`. Behavior running.
                Exit TEST8_S1.
                Transition action `` for TEST8_S1 to TEST8_G.<EntryPoint>(3).
                Enter TEST8_G.
                Transition action `count += 0;` for TEST8_G.<EntryPoint>(3) to TEST8_G.<EntryPoint>(1).
                Transition action `` for TEST8_G.<EntryPoint>(1) to {expectedState}.
                Enter {expectedState}.
            "));
        }
        else if (variation == 2)
        {
            // 
            tester.AddEventHandling("EV3", t => t(@$"
                State TEST8_S1: check behavior `EV3 TransitionTo(TEST8_G.<EntryPoint>(3))`. Behavior running.
                Exit TEST8_S1.
                Transition action `` for TEST8_S1 to TEST8_G.<EntryPoint>(3).
                Enter TEST8_G.
                Transition action `count += 0;` for TEST8_G.<EntryPoint>(3) to TEST8_G.<EntryPoint>(1).
                Transition action `` for TEST8_G.<EntryPoint>(1) to {expectedState}.
                Enter {expectedState}.
            "));
        }
        else
        {
            throw new Exception("unsupported variation " + variation);
        }

        // 
        tester.AddEventHandling("EV2", t => t(@$"
            State TEST8_G: check behavior `EV2 TransitionTo(TEST8_ROOT.<EntryPoint>(1))`. Behavior running.
            Exit {expectedState}.
            Exit TEST8_G.
            Transition action `` for TEST8_G to TEST8_ROOT.<EntryPoint>(1).
            Transition action `` for TEST8_ROOT.<EntryPoint>(1) to TEST8_S1.
            Enter TEST8_S1.
        "));
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
    }

    [Fact]
    public void Test9_Choice_3_2()
    {
        int incCount = 3;
        var expectedState = "TEST9_G_S3";
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
        tester.PreEvents = "EV9 EV1";

        for (int i = 0; i < incCount; i++)
        {
            // 
            tester.AddEventHandling("EV5", t => t(@"
                State TEST9_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            "));
        }

        // 
        tester.AddEventHandling("EV1", t => t(@$"
            State TEST9_S1_1: check behavior `EV1 TransitionTo(TEST9_S1.<ExitPoint>(1))`. Behavior running.
            Exit TEST9_S1_1.
            Transition action `` for TEST9_S1_1 to TEST9_S1.<ExitPoint>(1).
            Exit TEST9_S1.
            Transition action `` for TEST9_S1.<ExitPoint>(1) to {expectedState}.
            Enter {expectedState}.
        "));
    }

    ///////////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void Test9A_ExitPointTargetsParent()
    {
        tester.PreEvents = "EV9 EV2";

        tester.AddEventHandling("EV1", t => t(@"
            State TEST9A_S1_1: check behavior `EV1 TransitionTo(TEST9A_S1.<ExitPoint>(1))`. Behavior running.
            Exit TEST9A_S1_1.
            State TEST9A_S1_1: check behavior `exit / { count = 100; }`. Behavior running.
            Transition action `` for TEST9A_S1_1 to TEST9A_S1.<ExitPoint>(1).
            Exit TEST9A_S1.
            Transition action `count++;` for TEST9A_S1.<ExitPoint>(1) to TEST9A_S1.
            Transition action `` for TEST9A_S1.<InitialState> to TEST9A_S1_1.
            Enter TEST9A_S1_1.
            State TEST9A_S1_1: check behavior `enter [count == 0] / { clear_output(); }`. Behavior skipped.
        "));
    }

    ///////////////////////////////////////////////////////////////////////////////////

    // https://github.com/StateSmith/StateSmith/issues/132
    [Fact]
    public void Test9B_ExitPointExitOptimization()
    {
        tester.PreEvents = "EV9 EV3";

        tester.AddEventHandling("EV1", t => t(@"
            State TEST9B_ROOT: check behavior `EV1 TransitionTo(A4)`. Behavior running.
            Transition action `` for TEST9B_ROOT to A4.
            Enter A1.
            Enter A2.
            Enter A3.
            Enter A4.
        "));

        tester.AddEventHandling("EV1", t => t(@"
            State A4: check behavior `EV1 TransitionTo(A3.<ExitPoint>(1))`. Behavior running.
            Exit A4.
            Transition action `` for A4 to A3.<ExitPoint>(1).
            Exit A3.
            Exit A2.
            Exit A1.
            Transition action `` for A3.<ExitPoint>(1) to B4.
            Enter B1.
            Enter B2.
            Enter B3.
            Enter B4.
        "));
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
        tester.PreEvents = "EV10";

        for (int i = 0; i < incCount; i++)
        {
            tester.AddEventHandling("EV5", t => t(@"
                State TEST10_ROOT: check behavior `EV5 / { count++; }`. Behavior running.
            "));
        }

        var end = "";

        if (incCount == 0)
        {
            end = @$"
            Transition action `` for TEST10_G.<ChoicePoint>(1) to {expectedState}.
            Enter {expectedState}.
            ";
        }
        else if (incCount == 1 || incCount == 2)
        {
            end = @$"
            Transition action `` for TEST10_G.<ChoicePoint>(1) to TEST10_G.<ChoicePoint>(lower).
            Transition action `` for TEST10_G.<ChoicePoint>(lower) to {expectedState}.
            Enter {expectedState}.
            ";
        }
        else if (incCount == 3)
        {
            end = @$"
            Transition action `` for TEST10_G.<ChoicePoint>(1) to TEST10_G.<ChoicePoint>(upper).
            Transition action `` for TEST10_G.<ChoicePoint>(upper) to {expectedState}.
            Enter {expectedState}.
            ";
        }
        else if (incCount == 4)
        {
            end = @$"
            Transition action `` for TEST10_G.<ChoicePoint>(1) to TEST10_G.<ChoicePoint>(upper).
            Exit TEST10_G.
            Transition action `` for TEST10_G.<ChoicePoint>(upper) to {expectedState}.
            Enter {expectedState}.
            ";
        }
        else
        {
            throw new Exception("unsupported incCount " + incCount);
        }

        end = PrepExpectedOutput(end);

        if (variation == 1)
        {
            // 
            tester.AddEventHandling("EV1", t => t(@$"
                State TEST10_S1: check behavior `EV1 TransitionTo(TEST10_G.<EntryPoint>(1))`. Behavior running.
                Exit TEST10_S1.
                Transition action `` for TEST10_S1 to TEST10_G.<EntryPoint>(1).
                Enter TEST10_G.
                Transition action `` for TEST10_G.<EntryPoint>(1) to TEST10_G.<ChoicePoint>().
                Transition action `` for TEST10_G.<ChoicePoint>() to TEST10_G.<ChoicePoint>(1).
                {end}
            "));
        }
        else if (variation == 2)
        {
            // 
            tester.AddEventHandling("EV2", t => t(@$"
                State TEST10_S1: check behavior `EV2 TransitionTo(TEST10_G.<ChoicePoint>())`. Behavior running.
                Exit TEST10_S1.
                Transition action `` for TEST10_S1 to TEST10_G.<ChoicePoint>().
                Enter TEST10_G.
                Transition action `` for TEST10_G.<ChoicePoint>() to TEST10_G.<ChoicePoint>(1).
                {end}
            "));
        }
        else if (variation == 3)
        {
            // 
            tester.AddEventHandling("EV3", t => t(@$"
                State TEST10_S1: check behavior `EV3 TransitionTo(TEST10_G)`. Behavior running.
                Exit TEST10_S1.
                Transition action `` for TEST10_S1 to TEST10_G.
                Enter TEST10_G.
                Transition action `` for TEST10_G.<InitialState> to TEST10_G.<ChoicePoint>().
                Transition action `` for TEST10_G.<ChoicePoint>() to TEST10_G.<ChoicePoint>(1).
                {end}
            "));
        }
        else
        {
            throw new Exception("unsupported variation " + variation);
        }

        RunTester(); // required because this test is run in a loop
    }
}



