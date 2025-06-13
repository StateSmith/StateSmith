using System;
using System.Linq;
using FluentAssertions;
using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Validation;
using Xunit;

namespace StateSmithTest;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/470
/// </summary>
public class EventListUnitTests_470
{
    private static StateMachine BuildTestStateMachineForEvents(params string[] events)
    {
        var sm = new StateMachine(name: "SomeSm");
        var state1 = sm.AddChild(new State(name: "s1"));
        var initialState = sm.AddChild(new InitialState());
        initialState.AddTransitionTo(state1);

        state1.AddBehavior(new Behavior()
        {
            _triggers = events.ToList(),
            actionCode = $"doSomething();"
        });

        return sm;
    }

    private static void ExpectValidationFailure(string[] smEvents, string[] allowedEvents, string missing)
    {
        var (sm, mapping) = SetupStructures(smEvents, allowedEvents);
        Action action = () => ProcessEvents(sm, mapping);
        action.Should().Throw<BehaviorValidationException>()
            .WithMessage($"Event `{missing}` was not specified*");
    }

    private static void ExpectValidationSuccess(string[] smEvents, string[] allowedEvents)
    {
        var (sm, mapping) = SetupStructures(smEvents, allowedEvents);
        ProcessEvents(sm, mapping);
    }

    private static void ProcessEvents(StateMachine sm, EventMapping mapping)
    {
        EventFinalizer.Process(sm, mapping);
    }

    private static (StateMachine sm, EventMapping mapping) SetupStructures(string[] smEvents, string[] allowedEvents)
    {
        StateMachine sm = BuildTestStateMachineForEvents(smEvents);
        EventMapping mapping = new();

        foreach (var ev in allowedEvents)
        {
            mapping.AddEventValueMapping(ev);
        }

        return (sm, mapping);
    }

    [Fact]
    public void ValidationSuccesses()
    {
        // matches exactly
        ExpectValidationSuccess(smEvents: ["ev1", "ev2", "ev3"], allowedEvents: ["ev1", "ev2", "ev3"]);

        // allowed has 1 more
        ExpectValidationSuccess(smEvents: ["ev1", "ev2", "ev3"], allowedEvents: ["ev1", "ev2", "ev3", "ev4"]);

        // allowed event list is unspecified
        ExpectValidationSuccess(smEvents: ["ev1", "ev2", "ev3"], allowedEvents: []);

        // no state machine events found in diagram, should default to 'DO'
        ExpectValidationSuccess(smEvents: [], allowedEvents: [TriggerHelper.TRIGGER_DO]);
    }

    [Fact]
    public void ValidationFailures()
    {
        ExpectValidationFailure(missing: "ev3", smEvents: ["ev1", "ev2", "ev3"], allowedEvents: ["ev1", "ev2"]);
        ExpectValidationFailure(missing: "do", smEvents: ["do"], allowedEvents: ["ev1"]);
    }
}
