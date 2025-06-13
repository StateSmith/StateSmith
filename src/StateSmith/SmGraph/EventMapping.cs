using System;
using System.Collections.Generic;
using System.Linq;
using StateSmith.Common;

#nullable enable

namespace StateSmith.SmGraph;

/// <summary>
/// Maps event names to optional unique ids. Event ID values are allowed to be words like "SYSTEM_EV1" so that users
/// can reference externally defined values.
/// https://github.com/StateSmith/StateSmith/issues/470
/// </summary>
public class EventMapping
{
    public IReadOnlyDictionary<string, string> UnsanitizedMap => _unsanitizedEventMap;
    public bool IsEmpty => _unsanitizedEventMap.Count == 0;

    public IReadOnlyList<string> OrderedSanitizedEvents => _eventOrder;

    private readonly List<string> _eventOrder = new();

    /// <summary>
    /// Case insensitive dictionary. Contains original user casing. Not sanitized.
    /// Prefer using readonly field <see cref="UnsanitizedMap"/> when possible.
    /// </summary>
    private readonly Dictionary<string, string> _unsanitizedEventMap = new(StringComparer.OrdinalIgnoreCase);

    public bool EventHasExplicitValue(string eventName)
    {
        string eventValue = UnsanitizedMap[eventName];
        return IsExplicitEventValue(eventValue);
    }

    public static bool IsExplicitEventValue(string eventValue)
    {
        return string.IsNullOrWhiteSpace(eventValue) == false;
    }

    public bool ContainsEvent(string eventName)
    {
        return _unsanitizedEventMap.ContainsKey(eventName);
    }

    public string GetEventValue(string eventName)
    {
        return _unsanitizedEventMap[eventName];
    }

    public void UpdateValue(string eventName, string value)
    {
        if (!UnsanitizedMap.ContainsKey(eventName))
        {
            throw new ArgumentException($"Event `{eventName}` has not been mapped. You must add it first.");
        }

        _unsanitizedEventMap[eventName] = value;
    }

    public void AddEventValueMapping(string eventName, string id = "")
    {
        if (UnsanitizedMap.ContainsKey(eventName))
        {
            throw new ArgumentException($"Event `{eventName}` has already been mapped. Each event can only be specified once.");
        }

        if (!TriggerHelper.IsEvent(eventName))
        {
            throw new ArgumentException($"Invalid event mapping for `{eventName}`. That is a reserved trigger and not an event.");
        }

        _unsanitizedEventMap[eventName] = id;
        _eventOrder.Add(TriggerHelper.SanitizeTriggerName(eventName));
    }

    /// <summary>
    /// Only do if there's no user specified events
    /// </summary>
    public void DefaultSortEventOrdering()
    {
        _eventOrder.Sort((a, b) =>
            {
                if (a == TriggerHelper.TRIGGER_DO) return -1; // put DO first
                if (b == TriggerHelper.TRIGGER_DO) return 1; // put DO first
                return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            });
    }
}
