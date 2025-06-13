using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

/// <summary>
/// Maps event names to optional unique ids. Event ID values are allowed to be words like "SYSTEM_EV1" so that users
/// can reference externally defined values.
/// https://github.com/StateSmith/StateSmith/issues/470
/// </summary>
public class EventMapping
{
    public IReadOnlyDictionary<string, string> Map => _map;
    public bool IsEmpty => _map.Count == 0;

    /// <summary>
    /// Case insensitive dictionary.
    /// Prefer using readonly field <see cref="Map"/> when possible.
    /// </summary>
    private readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

    public bool EventHasExplicitValue(string eventName)
    {
        string eventValue = Map[eventName];
        return IsExplicitEventValue(eventValue);
    }

    public static bool IsExplicitEventValue(string eventValue)
    {
        return string.IsNullOrWhiteSpace(eventValue) == false;
    }

    public void AddEventValueMapping(string eventName, string id = "")
    {
        if (Map.ContainsKey(eventName))
        {
            throw new ArgumentException($"Event `{eventName}` has already been mapped. Each event can only be specified once.");
        }

        _map[eventName] = id;
    }
}
