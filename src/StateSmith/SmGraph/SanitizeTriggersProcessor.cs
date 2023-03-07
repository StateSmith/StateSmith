using StateSmith.Common;

#nullable enable

namespace StateSmith.SmGraph;

/// <summary>
/// Sanitizes all triggers. Currently not used because it changes how user sees generated code.
/// </summary>
public class SanitizeTriggersProcessor
{
    public static void Process(StateMachine sm)
    {
        sm.VisitTypeRecursively<Vertex>((vertex) =>
        {
            foreach (var behavior in vertex.Behaviors)
            {
                behavior._triggers = TriggerHelper.GetSanitizedTriggerList(behavior);
            }
        });
    }
}
