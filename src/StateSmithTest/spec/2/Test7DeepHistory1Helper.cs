using Spec;

// spell-checker: ignore battlebot

namespace StateSmithTest.spec2;

public class Test7DeepHistory1Helper
{
    SpecTester tester;

    public readonly string EventAlienDetected = "EV6"; 
    public readonly string EventAlienDetectedWantHero = "EV7"; 
    public readonly string EventAlienDone = "EV8";
    public readonly string EventDown = "EV1";
    public readonly string EventUp = "EV2";
    public readonly string BuildVarName = "sm.vars.BUILD_history";
    public readonly string BuildEnumAccess = "BUILD_HistoryId.";
    public readonly string AliensVarName = "sm.vars.ALIENS_DETECTED_history";
    public readonly string AliensEnumAccess = "ALIENS_DETECTED_HistoryId.";
    public readonly string GetBackupVarName = "sm.vars.GET_BACKUP_history";
    public readonly string GetBackupEnumAccess = "GET_BACKUP_HistoryId.";

    public Test7DeepHistory1Helper(SpecTester tester)
    {
        this.tester = tester;
    }

    public string ExpectBuildToAliensExiting(params string[] statesToExit)
    {
        return $@"
            State BUILD: check behavior `EV6 TransitionTo(ALIENS_DETECTED)`. Behavior running.
            Exit {string.Join(".\nExit ", statesToExit)}.
            Exit BUILD.
            Transition action `` for BUILD to ALIENS_DETECTED.
            Enter ALIENS_DETECTED.
            Transition action `` for ALIENS_DETECTED.InitialState to ALIENS_DETECTED.History.
        ".Trim();
    }

    public void StartToToyRaceCar()
    {
        tester.PreEvents = "EV7 EV3";
    }

    public void StartToToyBear()
    {
        StartToToyRaceCar();

        tester.AddEventHandling("EV1", t => t($@"
            State RACE_CAR: check behavior `EV1 TransitionTo(TEDDY_BEAR)`. Behavior running.
            Exit RACE_CAR.
            Transition action `` for RACE_CAR to TEDDY_BEAR.
            Enter TEDDY_BEAR.
            State TEDDY_BEAR: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}TEDDY_BEAR; }}`. Behavior running.
        "));
    }

    public void StartToToyGlowWorm()
    {
        StartToToyBear();

        tester.AddEventHandling("EV1", t => t($@"
            State TEDDY_BEAR: check behavior `EV1 TransitionTo(GLOW_WORM)`. Behavior running.
            Exit TEDDY_BEAR.
            Transition action `` for TEDDY_BEAR to GLOW_WORM.
            Enter GLOW_WORM.
            State GLOW_WORM: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}GLOW_WORM; }}`. Behavior running.
        "));
    }

    public void StartToToyBattleBot()
    {
        StartToToyGlowWorm();

        tester.AddEventHandling("EV1", t => t($@"
            State GLOW_WORM: check behavior `EV1 TransitionTo(ROBOT)`. Behavior running.
            Exit GLOW_WORM.
            Transition action `` for GLOW_WORM to ROBOT.
            Enter ROBOT.
            State ROBOT: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}ROBOT; }}`. Behavior running.
            Transition action `` for ROBOT.InitialState to BATTLEBOT.
            Enter BATTLEBOT.
            State BATTLEBOT: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}BATTLEBOT; }}`. Behavior running.
        "));
    }

    public void StartToToyWallE()
    {
        StartToToyBattleBot();

        tester.AddEventHandling("EV1", t => t($@"
            State BATTLEBOT: check behavior `EV1 TransitionTo(WALL_E)`. Behavior running.
            Exit BATTLEBOT.
            Transition action `` for BATTLEBOT to WALL_E.
            Enter WALL_E.
            State WALL_E: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}WALL_E; }}`. Behavior running.
        "));
    }

    public void StartToTool_ImpactDrill()
    {
        StartToToyWallE();

        tester.AddEventHandling("EV1", t => t($@"
            State TOY: check behavior `EV1 TransitionTo(TOOL)`. Behavior running.
            Exit WALL_E.
            Exit ROBOT.
            Exit TOY.
            Transition action `` for TOY to TOOL.
            {EnterToolText()}
            Transition action `` for TOOL.InitialState to IMPACT_DRILL.
            { EnterImpactDrillText() }
        "));
    }

    public string EnterToolText()
    {
        return $@"
            Enter TOOL.
            State TOOL: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}TOOL; }}`. Behavior running.
        ".Trim();
    }

    public void StartToTool_CircularSaw()
    {
        StartToTool_ImpactDrill();
        ImpactDrill_to_CircularSaw();
    }

    public void ImpactDrill_to_CircularSaw()
    {
        tester.AddEventHandling("EV1", t => t($@"
            State IMPACT_DRILL: check behavior `EV1 TransitionTo(CIRCULAR_SAW)`. Behavior running.
            Exit IMPACT_DRILL.
            Transition action `` for IMPACT_DRILL to CIRCULAR_SAW.
            { EnterCircularSawText() }
        "));
    }

    public string EnterCircularSawText()
    {
        return $@"
            Enter CIRCULAR_SAW.
            State CIRCULAR_SAW: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}CIRCULAR_SAW; }}`. Behavior running.
        ".Trim();
    }

    public string EnterImpactDrillText()
    {
        return $@"
            Enter IMPACT_DRILL.
            State IMPACT_DRILL: check behavior `enter / {{ {BuildVarName} = {BuildEnumAccess}IMPACT_DRILL; }}`. Behavior running.
        ".Trim();
    }

    public void SnowballFight_to_GiveCookies()
    {
        tester.AddEventHandling("EV1", t => t($@"
             State SNOWBALL_FIGHT: check behavior `EV1 TransitionTo(GIVE_COOKIES)`. Behavior running.
             Exit SNOWBALL_FIGHT.
             Transition action `` for SNOWBALL_FIGHT to GIVE_COOKIES.
             {EnterGiveCookiesText()}
         "));
    }

    public string EnterGiveCookiesText()
    {
        return $@"
            Enter GIVE_COOKIES.
            State GIVE_COOKIES: check behavior `enter / {{ {AliensVarName} = {AliensEnumAccess}GIVE_COOKIES; }}`. Behavior running.
        ".Trim();
    }

    public void GiveCookies_to_CallThor()
    {
        tester.AddEventHandling("EV1", t => t($@"
             State GIVE_COOKIES: check behavior `EV1 TransitionTo(CALL_THOR)`. Behavior running.
             Exit GIVE_COOKIES.
             Transition action `` for GIVE_COOKIES to CALL_THOR.
             {EnterGetBackupText()}
             {EnterHeroText()}
             Enter CALL_THOR.
         "));
    }

    public string EnterGetBackupText()
    {
        return $@"
             Enter GET_BACKUP.
        ".Trim();
    }

    public string EnterHeroText()
    {
        return $@"
             Enter HERO.
             State HERO: check behavior `enter / {{ {AliensVarName} = {AliensEnumAccess}HERO; }}`. Behavior running.
             State HERO: check behavior `enter / {{ {GetBackupVarName} = {GetBackupEnumAccess}HERO; }}`. Behavior running.
        ".Trim();
    }

    public void CallThor_to_CallBatman()
    {
        tester.AddEventHandling("EV1", t => t($@"
             State CALL_THOR: check behavior `EV1 TransitionTo(CALL_BATMAN)`. Behavior running.
             Exit CALL_THOR.
             Transition action `` for CALL_THOR to CALL_BATMAN.
             Enter CALL_BATMAN.
         "));
    }

    public void CallBatman_to_BuddyElf()
    {
        tester.AddEventHandling("EV1", t => t($@"
             State CALL_BATMAN: check behavior `EV1 TransitionTo(BUDDY_ELF)`. Behavior running.
             Exit CALL_BATMAN.
             Exit HERO.
             Transition action `` for CALL_BATMAN to BUDDY_ELF.
             {EnterLocalHelpText()}
             {EnterBuddyELfText()}
         "));
    }

    public string EnterBuddyELfText()
    {
        return $@"
             Enter BUDDY_ELF.
             State BUDDY_ELF: check behavior `enter / {{ {AliensVarName} = {AliensEnumAccess}BUDDY_ELF; }}`. Behavior running.
             State BUDDY_ELF: check behavior `enter / {{ {GetBackupVarName} = {GetBackupEnumAccess}BUDDY_ELF; }}`. Behavior running.
        ".Trim();
    }

    public string EnterLocalHelpText()
    {
        return $@"
             Enter LOCAL_HELP.
        ".Trim();
    }
}



