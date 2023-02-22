using StateSmith.Common;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/108
/// </summary>
public class SupportAlternateTriggersProcessor
{
    public static void Process(StateMachine sm)
    {
        sm.VisitTypeRecursively<Vertex>((vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                var sanitizedTriggers = behavior.SanitizedTriggers.ToList();

                for (int i = 0; i < sanitizedTriggers.Count; i++)
                {
                    string trigger = sanitizedTriggers[i];
                    switch (trigger)
                    {
                        case "entry":
                            behavior._triggers[i] = TriggerHelper.TRIGGER_ENTER;
                            break;
                        // may also want to support "en" for "enter" and "ex" for "exit"
                    }
                }
            }
        });
    }
}
