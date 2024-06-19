#nullable enable

using StateSmith.Output.UserConfig;
using System.Collections.Generic;
using StateSmith.Common;
using System;

namespace StateSmith.SmGraph.TriggerMap;

/// <summary>
/// Parses an event map like `ALL => *` and then replaces all use of `ALL` with all events in the design.
/// https://github.com/StateSmith/StateSmith/issues/161
/// </summary>
public class TriggerMapProcessor
{
    readonly TriggerMapper triggerMapper = new();

    readonly RenderConfigBaseVars configVars;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configVars"></param>
    public TriggerMapProcessor(RenderConfigBaseVars configVars)
    {
        this.configVars = configVars;
    }

    public void Process(StateMachine sm)
    {
        var mapText = configVars.TriggerMap;
        HashSet<string> events = CollectSmEvents(sm);
        triggerMapper.Setup(mapText, events);

        sm.VisitRecursively(v =>
        {
            foreach (var behavior in v.Behaviors)
            {
                behavior._triggers = triggerMapper.MapTriggers(behavior.Triggers);
            }
        });
    }

    public static HashSet<string> CollectSmEvents(StateMachine sm)
    {
        HashSet<string> events = new(StringComparer.OrdinalIgnoreCase);

        sm.VisitRecursively(v =>
        {
            foreach (var behavior in v.Behaviors)
            {
                foreach (var trigger in behavior.Triggers)
                {
                    if (TriggerHelper.IsEvent(trigger))
                    {
                        events.Add(trigger);
                    }
                }
            }
        });
        return events;
    }


}
