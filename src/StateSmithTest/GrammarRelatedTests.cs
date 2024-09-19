using FluentAssertions;
using StateSmith.Input.Antlr4;
using System;
using Xunit;

namespace StateSmithTest;

/// <summary>
/// See also antlr tests
/// </summary>
public class GrammarRelatedTests
{
    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/277
    /// </summary>
    [Fact]
    public void AllowExitInGuardActionCode_277()
    {
        var plantUmlText = """
            @startuml ExampleSm
            state c1 {
                c1: [exit] / exit();
                c1: event1 / exit();
                c1: event1 / exit = 2;
                c1: event1 / system.exit();
                c1: event1 / EXIT();
                c1: event1 / EXIT = 22;
                c1: event1 / system.EXIT();
            }
            [*] --> c1
            @enduml
            """;
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/277
    /// </summary>
    [Fact]
    public void AllowEntryInGuardActionCode_277()
    {
        var plantUmlText = """
            @startuml ExampleSm
            state c1 {
                c1: [entry] / entry();
                c1: event1 / entry();
                c1: event1 / entry = 2;
                c1: event1 / system.entry();
                c1: event1 / ENTRY();
            }
            [*] --> c1
            @enduml
            """;
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText);
    }


    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/211
    /// </summary>
    [Fact]
    public void AllowViaInGuardActionCode_211()
    {
        var plantUmlText = """

            @startuml ExampleSm
            state c1 {
                c1: [via] / via();
                c1: event1 / via();
                c1: event1 / via = 2;
                c1: event1 / system.via();
                c1: event1 / VIA();
                c1: event1 / VIA = 22;
                c1: event1 / system.VIA();
            }
            [*] --> c1
            @enduml

            """;

        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/330
    /// </summary>
    [Fact]
    public void BracesFileNameAllowed_330()
    {
        var plantUmlText = """
            @startuml {fileName}
            [*] --> c1
            @enduml
            """;
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText);

        plantUmlText = """
            @startuml {fileName}_2
            [*] --> c1
            @enduml
            """;
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/330
    /// </summary>
    [Fact]
    public void BadBracesThrows_330()
    {
        var plantUmlText = """
            @startuml {{fileName}
            [*] --> c1
            @enduml
            """;
        Assert.Throws<FormatException>(() => TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText));

        plantUmlText = """
            @startuml {fileName}}
            [*] --> c1
            @enduml
            """;
        Assert.Throws<FormatException>(() => TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText));
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/282
    /// </summary>
    [Fact]
    public void MultipleSingleQuoteStrings_282()
    {
        const string actionCode = "print('some message'); logger(format('another message'));";

        // below is problematic because StateSmith essentially sees this: `print('string-content'));` and it has unbalanced parentheses
        var plantUmlText = $$"""
            @startuml {fileName}
            [*] --> c1
            c1: enter / {{actionCode}}
            @enduml
            """;
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText); // shouldn't throw an exception

        ActionCodeInspector actionCodeInspector = new();
        actionCodeInspector.Parse(actionCode);
        actionCodeInspector.functionsCalled.Should().BeEquivalentTo(["print", "logger", "format"]);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/282
    /// </summary>
    [Fact]
    public void MultipleBackTickStrings_282()
    {
        const string actionCode = "print(`some message`); logger(format(`another message`));";

        // below is problematic because StateSmith essentially sees this: <code>print(`string-content`));</code> and it has unbalanced parentheses
        var plantUmlText = $$"""
            @startuml {fileName}
            [*] --> c1
            c1: enter / {{actionCode}}
            @enduml
            """;
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText); // shouldn't throw an exception

        ActionCodeInspector actionCodeInspector = new();
        actionCodeInspector.Parse(actionCode);
        actionCodeInspector.functionsCalled.Should().BeEquivalentTo(["print", "logger", "format"]);
    }
}
