#nullable enable

namespace StateSmith.SmGraph.Validation;

/// <summary>
/// The purpose of this class is to ensure that only expected events are found in diagram.
/// Inputs: found events, allowed events
/// </summary>
public class EventValidator
{
    public static void Validate(string foundEventName, Behavior owningBehavior, EventMapping mapping)
    {
        // If user hasn't predefined any events, then we don't need to validate.
        if (mapping.IsEmpty)
        {
            return;
        }

        if (!mapping.Map.ContainsKey(foundEventName))
        {
            throw new BehaviorValidationException(
                owningBehavior,
                $"Event `{foundEventName}` was not specified in user event mapping. " +
                $"Allowed events are: `{string.Join(", ", mapping.Map.Keys)}`. " +
                $"If you want to use this event, please add it to the allowed events list. " +
                $"Info: https://github.com/StateSmith/StateSmith/issues/470 ."
            );
        }
    }
}
