using StateSmith.Common;

#nullable enable

namespace StateSmith.SmGraph;

public class SupportAlternateTriggersProcessor
{
    public static void Process(StateMachine sm)
    {
        sm.VisitTypeRecursively<Vertex>((vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                for (int i = 0; i < behavior.triggers.Count; i++)
                {
                    string trigger = behavior.triggers[i];
                    switch (trigger.ToLower())
                    {
                        case "entry":
                            behavior.triggers[i] = TriggerHelper.TRIGGER_ENTER;
                            break;
                        // may also want to support "en" for "enter" and "ex" for "exit"
                    }
                }
            }
        });
    }
}
