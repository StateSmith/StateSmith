#nullable enable

using System;
using System.Collections.Generic;

namespace StateSmith.SmGraph.TriggerMap;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/161
/// </summary>
public class TriggerMapRule
{
    /// <summary>
    /// In the below mapping, `UPx` is the rule `name`.
    /// <code>
    /// UPx => UP_PRESS, UP_HELD
    /// </code>
    /// </summary>
    public string name = "";

    /// <summary>
    /// A trigger matcher can be a string like `EV1`. This will match `EV1` exactly.
    /// It can also be a JavaScript like regex `/EV\d+/`. This will match `EV1`, `EV2`, `EV3`, ...
    /// It can also be a wild card match `EV*`. This will match `EV`, `EVENT`, `EV3`, ...
    /// Trigger matches can target any trigger like enter,exit,do...
    /// </summary>
    public HashSet<string> triggerMatchers = new(StringComparer.OrdinalIgnoreCase);
}
