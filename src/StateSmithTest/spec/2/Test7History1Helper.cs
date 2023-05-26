using Spec;

namespace StateSmithTest.spec2;

public class Test7History1Helper
{
    SpecTester tester;

    public const string SmVars = "this.vars";

    public readonly string EventOnToOff = "EV6"; 
    public readonly string EventOffToOn = "EV7";
    public readonly string OnVarName = $"{SmVars}.ON_history";
    public readonly string OnEnumAccess = "ON_HistoryId.";
    public readonly string OffVarName = $"{SmVars}.OFF_history";
    public readonly string OffEnumAccess = "OFF_HistoryId.";

    public const string GilStart = "$gil(";
    public const string GilEnd = ")";

    public Test7History1Helper(SpecTester tester)
    {
        this.tester = tester;
    }

    public void StartToStateOn1()
    {
        tester.PreEvents = "EV7 EV2";

        tester.AddEventHandling("EV1", t => t($@"
            State ON1: check behavior `EV1 TransitionTo(ON2)`. Behavior running.
            Exit ON1.
            Transition action `` for ON1 to ON2.
            Enter ON2.
            State ON2: check behavior `enter / {{ {GilStart}{OnVarName} = {OnEnumAccess}ON2;{GilEnd} }}`. Behavior running.
        "));
    }

    public void StartToStateOff1()
    {
        StartToStateOn1();

        tester.AddEventHandling("EV6", t => t($@"
            State ON: check behavior `EV6 TransitionTo(OFF)`. Behavior running.
            Exit ON2.
            Exit ON.
            Transition action `` for ON to OFF.
            Enter OFF.
            Transition action `` for OFF.<InitialState> to OFF.<History>.
            Transition action `` for OFF.<History> to OFF1.
            Enter OFF1.
            State OFF1: check behavior `enter / {{ {GilStart}{OffVarName} = {OffEnumAccess}OFF1;{GilEnd} }}`. Behavior running.
        "));
    }

    public void StartToStateOff2()
    {
        StartToStateOff1();

        tester.AddEventHandling("EV1", t => t($@"
            State OFF1: check behavior `EV1 TransitionTo(OFF2)`. Behavior running.
            Exit OFF1.
            Transition action `` for OFF1 to OFF2.
            Enter OFF2.
            State OFF2: check behavior `enter / {{ {GilStart}{OffVarName} = {OffEnumAccess}OFF2;{GilEnd} }}`. Behavior running.
        "));
    }

    public void StartToStateOff3()
    {
        StartToStateOff2();

        tester.AddEventHandling("EV1", t => t($@"
            State OFF2: check behavior `EV1 TransitionTo(OFF3)`. Behavior running.
            Exit OFF2.
            Transition action `` for OFF2 to OFF3.
            Enter OFF3.
            State OFF3: check behavior `enter / {{ {GilStart}{OffVarName} = {OffEnumAccess}OFF3;{GilEnd} }}`. Behavior running.
        "));
    }
}



