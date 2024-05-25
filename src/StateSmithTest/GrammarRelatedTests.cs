using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph.Validation;
using StateSmithTest.Output;
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
    public void AllowExitInActionCode_277()
    {
        var plantUmlText = """
            @startuml ExampleSm
            state c1 {
                c1: SOME_EVENT / exit(); 
            }
            [*] --> c1
            @enduml
            """;
        TestHelper.BuildSmRunnerForPlantUmlString(plantUmlText).Run();
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/277
    /// </summary>
    [Fact]
    public void AllowEntryInActionCode_277()
    {
        var plantUmlText = """
            @startuml ExampleSm
            state c1 {
                c1: SOME_EVENT / entry(); 
            }
            [*] --> c1
            @enduml
            """;
        TestHelper.BuildSmRunnerForPlantUmlString(plantUmlText).Run();
    }
}
