#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph.Validation;
using System.Linq;

namespace StateSmith.SmGraph;

public class EventFinalizer
{
    public static void Process(StateMachine stateMachine, EventMapping mapping)
    {
        AddEventsFoundInDiagram(stateMachine, mapping);
        AddDefaultEventIfNeeded(stateMachine, mapping);
        FinalizeEventMapping(stateMachine, mapping);
    }

    private static void AddEventsFoundInDiagram(StateMachine stateMachine, EventMapping mapping)
    {
        stateMachine.VisitRecursively(v =>
        {
            foreach (var behavior in v.Behaviors)
            {
                foreach (var trigger in behavior.Triggers)
                {
                    bool wasAdded = TriggerHelper.MaybeAddEventToSm(stateMachine, behavior, trigger);

                    if (wasAdded)
                    {
                        EventValidator.Validate(trigger, behavior, mapping);
                    }
                }
            }
        });
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/121
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="mapping"></param>
    private static void AddDefaultEventIfNeeded(StateMachine stateMachine, EventMapping mapping)
    {
        bool noEventsFound = stateMachine.GetEventSet().Count == 0;
        bool userHasNotSpecifiedEventList = mapping.IsEmpty;

        if (noEventsFound && userHasNotSpecifiedEventList)
        {
            stateMachine._events.Add(TriggerHelper.TRIGGER_DO);
        }
    }

    public static void FinalizeEventMapping(StateMachine stateMachine, EventMapping mapping)
    {
        if (mapping.IsEmpty)
        {
            // we need to populate from state machine events
            foreach (var ev in stateMachine.GetEventSet())
            {
                mapping.AddEventValueMapping(ev);
            }
            // FIXME - need to sort? put do event first if it exists?
        }

        // FIXME - double check that state machine events match event mapping

        bool alreadyExplicitlyDefined = EventMapping.IsExplicitEventValue(mapping.Map.First().Value);
        if (alreadyExplicitlyDefined)
        {
            // If the first event is explicitly defined, then all events must be explicitly defined.
            // We have earlier validations that ensure this.
            return;
        }

        // we need assign explicit values to all events
        foreach (var keyValuePair in mapping.Map)
        {
            // FIXME finish here.
        }
    }
}
