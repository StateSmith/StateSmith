using FluentAssertions;
using StateSmith.SmGraph.Validation;
using System;
using System.Linq;
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

    [Fact]
    public void CodeGenImplicitOrdering()
    {
        var plantUml = """
            @startuml MySm
            [*] -> S1

            /'! $CONFIG : toml
                RenderConfig.EventCommaList = "EV1, ev2, do, ev3"
            '/
            @enduml
            """;

        var fakeFileSystem = TestHelper.CaptureSmRunnerFilesForPlantUmlString(plantUml, transpilerId: StateSmith.Runner.TranspilerId.C99);

        string code = fakeFileSystem.GetCapturesForFileName("MySm.h").Single().code;
        code.Should().Contain("MySm_EventId_EV1 = 0,");
        code.Should().Contain("MySm_EventId_EV2 = 1,");
        code.Should().Contain("MySm_EventId_DO = 2, // The `do` event is special");
        code.Should().Contain("MySm_EventId_EV3 = 3");
    }

    [Fact]
    public void CodeGenExplicitNumericOrdering()
    {
        var plantUml = """"
            @startuml MySm
            [*] -> S1

            /'! $CONFIG : toml
                RenderConfig.EventCommaList = """
                    EV1 = 100,
                    ev2 = 200,
                    do = 0,
                    ev3 = 300
                    """
            '/
            @enduml
            """";

        var fakeFileSystem = TestHelper.CaptureSmRunnerFilesForPlantUmlString(plantUml, transpilerId: StateSmith.Runner.TranspilerId.C99);

        string code = fakeFileSystem.GetCapturesForFileName("MySm.h").Single().code;
        code.Should().Contain("MySm_EventId_EV1 = 100,");
        code.Should().Contain("MySm_EventId_EV2 = 200,");
        code.Should().Contain("MySm_EventId_DO = 0,");
        code.Should().Contain("MySm_EventId_EV3 = 300");
    }

    [Fact]
    public void CodeGenExplicitOrderingWithExternalUserDefines()
    {
        var plantUml = """"
            @startuml MySm
            [*] -> S1

            /'! $CONFIG : toml
                RenderConfig.EventCommaList = """
                    EV1 = EXTERNAL_SYS1,
                    ev2 = EXTERNAL_SYS2,
                    do = 0,
                    ev3 = EXTERNAL_SYS3
                    """
            '/
            @enduml
            """";

        var fakeFileSystem = TestHelper.CaptureSmRunnerFilesForPlantUmlString(plantUml, transpilerId: StateSmith.Runner.TranspilerId.C99);

        string code = fakeFileSystem.GetCapturesForFileName("MySm.h").Single().code;
        code.Should().Contain("MySm_EventId_EV1 = EXTERNAL_SYS1,");
        code.Should().Contain("MySm_EventId_EV2 = EXTERNAL_SYS2,");
        code.Should().Contain("MySm_EventId_DO = 0,");
        code.Should().Contain("MySm_EventId_EV3 = EXTERNAL_SYS3");
    }
}
