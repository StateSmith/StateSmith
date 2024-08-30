using StateSmith.SmGraph;
using StateSmith.SmGraph.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Common;

/// <summary>
/// A trigger can be something like "enter", "exit", the "do" event, or a user specified event. 
/// Note that the triggers should be treated as case insensitive. Use <see cref="TriggerHelper.SanitizeTriggerName(string)"/>.
/// </summary>
public static class TriggerHelper
{
    /// <summary>
    /// Trigger name for behavior that is run when a state is entered.
    /// </summary>
    public const string TRIGGER_ENTER = "enter";

    /// <summary>
    /// Trigger name for behavior that is run when a state is exited.
    /// </summary>
    public const string TRIGGER_EXIT = "exit";

    /// <summary>
    /// The do event is special in that it is not normally consumed like other events.
    /// </summary>
    public const string TRIGGER_DO = "do";

    /// <summary>
    /// Sanitizes trigger before check.
    /// </summary>
    /// <param name="triggerName">Can be unsanitized.</param>
    /// <returns></returns>
    public static bool IsEnterExitTrigger(string triggerName)
    {
        string trigger = SanitizeTriggerName(triggerName);
        return InnerIsEnterExitTrigger(trigger);
    }

    public static bool IsEnterTrigger(string triggerName)
    {
        string trigger = SanitizeTriggerName(triggerName);
        return trigger == TRIGGER_ENTER;
    }

    /// <summary>
    /// Events are triggers that are not the enter/exit trigger.
    /// Sanitizes trigger before check.
    /// </summary>
    /// <param name="triggerName">Can be unsanitized.</param>
    /// <returns></returns>
    public static bool IsEvent(string triggerName)
    {
        return IsEnterExitTrigger(triggerName) == false;
    }

    /// <summary>
    /// Sanitizes trigger before check.
    /// </summary>
    /// <param name="triggerName">Can be unsanitized.</param>
    /// <returns></returns>
    public static bool IsDoEvent(string triggerName)
    {
        triggerName = SanitizeTriggerName(triggerName);
        return triggerName == TRIGGER_DO;
    }

    /// <summary>
    /// StateSmith treats triggers/events as case insensitive.
    /// </summary>
    /// <param name="triggerName"></param>
    /// <returns>The trigger name trimmed and converted to lowercase.</returns>
    public static string SanitizeTriggerName(string triggerName)
    {
        return triggerName.ToLower().Trim();
    }

    /// <summary>
    /// Adds event to state machine if needed. Sanitizes trigger before check.
    /// </summary>
    /// <param name="sm"></param>
    /// <param name="behavior"></param>
    /// <param name="triggerName">Can be unsanitized</param>
    /// <exception cref="BehaviorValidationException"></exception>
    public static void MaybeAddEventToSm(StateMachine sm, Behavior behavior, string triggerName)
    {
        var eventSet = sm._events;
        MaybeAddEventToSet(eventSet, behavior, triggerName);
    }

    public static void MaybeAddEventToSet(HashSet<string> eventSet, Behavior behavior, string triggerName)
    {
        string cleanTrigger = SanitizeTriggerName(triggerName);

        if (cleanTrigger.Length == 0)
        {
            throw new BehaviorValidationException(behavior, "Has a blank trigger");
        }

        if (InnerIsEnterExitTrigger(cleanTrigger))
        {
            return;
        }

        eventSet.Add(cleanTrigger);
    }

    /// <summary>
    /// Returns a unique set of sanitized triggers.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static HashSet<string> GetSanitizedTriggersSet(Vertex state)
    {
        HashSet<string> triggerNames = new();
        foreach (var b in state.Behaviors)
        {
            foreach (var trigger in b.Triggers)
            {
                triggerNames.Add(SanitizeTriggerName(trigger));
            }
        }

        return triggerNames;
    }

    public static List<string> GetSanitizedTriggerList(Behavior behavior)
    {
        List<string> triggerNames = new();
        foreach (var trigger in behavior.Triggers)
        {
            triggerNames.Add(SanitizeTriggerName(trigger));
        }

        return triggerNames;
    }

    /// <summary>
    /// These do NOT include entry and exit triggers
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static HashSet<string> GetEvents(NamedVertex state)
    {
        var triggerNames = GetSanitizedTriggersSet(state);
        triggerNames.Remove(TRIGGER_ENTER);
        triggerNames.Remove(TRIGGER_EXIT);
        return triggerNames;
    }

    /// <summary>
    /// Sanitizes triggers.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="triggerName">Can be unsanitized.</param>
    /// <returns></returns>
    public static IEnumerable<Behavior> GetBehaviorsWithTrigger(Vertex vertex, string triggerName)
    {
        triggerName = SanitizeTriggerName(triggerName);
        return vertex.Behaviors.Where(b => b.Triggers.Any(t => SanitizeTriggerName(t) == triggerName));
    }

    /// <summary>
    /// This function is special in that it can be called before transformations are applied,
    /// like the one that adds the implicit do trigger.
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool HasAnEventTrigger(Behavior b)
    {
        return b.Triggers.Any(t => IsEvent(t)) || b.HasImplicitDoTriggerOnly();
    }

    /// <summary>
    /// Sanitizes triggers.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="triggerName">Can be unsanitized.</param>
    /// <returns></returns>
    public static bool HasBehaviorsWithTrigger(Vertex vertex, string triggerName)
    {
        return GetBehaviorsWithTrigger(vertex, triggerName).Any();
    }

    private static bool InnerIsEnterExitTrigger(string sanitizedTrigger)
    {
        switch (sanitizedTrigger)
        {
            case TRIGGER_ENTER:
            case TRIGGER_EXIT:
                return true;
        }

        return false;
    }
}
