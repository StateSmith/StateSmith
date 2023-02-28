using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Common;

public static class TriggerHelper
{
    public const string TRIGGER_ENTER = "enter";
    public const string TRIGGER_EXIT = "exit";
    public const string TRIGGER_DO = "do";

    public static bool IsEnterExitTrigger(string triggerName)
    {
        string trigger = CleanTriggerName(triggerName);
        return InnerIsEnterExitTrigger(trigger);
    }

    public static bool IsEnterTrigger(string triggerName)
    {
        string trigger = CleanTriggerName(triggerName);
        return trigger == TRIGGER_ENTER;
    }

    public static bool IsEvent(string triggerName)
    {
        return IsEnterExitTrigger(triggerName) == false;
    }

    public static bool IsDoEvent(string trigger)
    {
        trigger = CleanTriggerName(trigger);
        return trigger == TRIGGER_DO;
    }

    private static bool InnerIsEnterExitTrigger(string trigger)
    {
        switch (trigger)
        {
            case TRIGGER_ENTER:
            // case "entry":    //todolow support
            case TRIGGER_EXIT:
                return true;
        }

        return false;
    }

    private static string CleanTriggerName(string triggerName)
    {
        return triggerName.ToLower().Trim();
    }

    public static void MaybeAddEvent(StateMachine sm, Behavior behavior, string triggerName)
    {
        string cleanTrigger = CleanTriggerName(triggerName);

        if (cleanTrigger.Length == 0)
        {
            throw new BehaviorValidationException(behavior, "Has a blank trigger");
        }

        if (InnerIsEnterExitTrigger(cleanTrigger))
        {
            return;
        }

        sm._events.Add(cleanTrigger);
    }

    // TODOLOW return sorted array instead
    public static HashSet<string> GetStateTriggers(NamedVertex state)
    {
        HashSet<string> triggerNames = new();
        foreach (var b in state.Behaviors)
        {
            foreach (var trigger in b.triggers)
            {
                triggerNames.Add(trigger);
            }
        }

        return triggerNames;
    }

    public static IEnumerable<Behavior> GetBehaviorsWithTrigger(Vertex vertex, string triggerName)
    {
        return vertex.Behaviors.Where(b => b.triggers.Contains(triggerName));
    }

    public static bool HasBehaviorsWithTrigger(Vertex vertex, string triggerName)
    {
        return vertex.Behaviors.Any(b => b.triggers.Contains(triggerName));
    }
}
