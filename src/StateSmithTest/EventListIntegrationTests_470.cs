using FluentAssertions;
using StateSmith.SmGraph.Validation;
using System;
using Xunit;

namespace StateSmithTest;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/470
/// </summary>
public class EventListIntegrationTests_470
{
    [Fact]
    public void Success_DefaultDoAndOthersAllowed()
    {
        var plantUml = """
            @startuml MySm
            [*] -> S1

            ' below has default do event
            S1 -> S2: [x > 10]

            ' just a transition with an event
            S2 -> S1: EV1

            /'! $CONFIG : toml
                RenderConfig.EventCommaList = "EV1, do"
            '/
            @enduml
            """;
        TestHelper.RunSmRunnerForPlantUmlString(plantUml);
    }

    [Fact]
    public void Fail_DefaultDoNotAllowed()
    {
        var plantUml = """
            @startuml MySm
            [*] -> S1

            ' below has default 'do' event, but isn't in event comma list
            S1 -> S2: [x > 10]

            /'! $CONFIG : toml
                RenderConfig.EventCommaList = "EV1 = 0, EV2 = 1"
            '/
            @enduml
            """;

        Action action = () => TestHelper.RunSmRunnerForPlantUmlString(plantUml);
        action.Should().Throw<BehaviorValidationException>()
            .WithMessage("Event `do` was not specified in user event mapping. Allowed events are: `EV1, EV2`. If you want to use this event, please add it to the allowed events list. Info: https://github.com/StateSmith/StateSmith/issues/470 .");
    }

    [Fact]
    public void Fail_Ev1NotAllowed()
    {
        var plantUml = """
            @startuml MySm
            [*] -> S1
            S1: ev1 / doStuff();

            /'! $CONFIG : toml
                RenderConfig.EventCommaList = "DO"
            '/
            @enduml
            """;

        Action action = () => TestHelper.RunSmRunnerForPlantUmlString(plantUml);
        action.Should().Throw<BehaviorValidationException>()
            .WithMessage("Event `ev1` was not specified*");
    }
}
