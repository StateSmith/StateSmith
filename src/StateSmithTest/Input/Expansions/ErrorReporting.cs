using Xunit;
using FluentAssertions;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;

namespace StateSmithTest.Input;

public class ErrorReporting
{
    [Fact]
    public void DiagramToSmConverter_PrintAndThrowIfNodeParseFail()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> MY_STATE
            MY_STATE: enter / printf("Hello";
            MY_STATE: exit / log("Hello";
            @enduml
            """;

        string consoleOutput = RunExpectGenericFailure(plantUmlText);

        consoleOutput.Should().Contain("""
            Exception FormatException : Failed parsing node label
            Parent path: `ROOT`
            Label: `MY_STATE
            enter / printf("Hello";
            exit / log("Hello";`
            Diagram id: `MY_STATE`
            Reason(s): mismatched input '/' expecting ')' at line 3 column 5. Offending symbol: `/`.
                       missing ')' at '<EOF>' at line 3 column 19. Offending symbol: `<EOF>`.
            """);
    }

    [Fact]
    public void PlantUml_BadInput()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [a] --> A A
            @enduml
            """;

        string consoleOutput = RunExpectGenericFailure(plantUmlText);

        consoleOutput.Should().Contain("""
            Exception FormatException : PlantUML input failed parsing.
            Reason(s): mismatched input 'a' expecting {'h', 'H'} at line 2 column 1. Offending symbol: `a`.
                       no viable alternative at input ' A' at line 2 column 10. Offending symbol: `A`.
            """);
    }

    [Fact]
    public void DiagramToSmConverter_PrintAndThrowIfEdgeParseFail()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> MyStateWithBadCode : EVENT / \ print(
            @enduml
            """;

        string consoleOutput = RunExpectGenericFailure(plantUmlText);

        consoleOutput.Should().Contain("""
            Failed parsing diagram edge
            from: ROOT.<InitialState>
            to:   ROOT.MyStateWithBadCode
            Edge label: `EVENT / \ print(`
            Reason(s): no viable alternative at input ' print(' at line 1 column 16. Offending symbol: `<EOF>`.
                       missing ')' at '<EOF>' at line 1 column 16. Offending symbol: `<EOF>`.
            Edge diagram id: line_2_column_0
            """);
    }

    [Fact]
    public void Printed_VertexValidationException()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1
            s1 --> [h]
            [h] --> s2
            [h] --> s3
            @enduml
            """;

        string consoleOutput = RunExpectGenericFailure(plantUmlText);

        consoleOutput.Should().Contain("""
            VertexValidationException: history vertex must only have a single default transition. Found 2 behaviors.
            Vertex:
                Path: ROOT.<History>
                Diagram Id: line_3_column_7
                Children count: 0
                Behaviors count: 2
                Incoming transitions count: 1
            """);
    }

    private static string RunExpectGenericFailure(string plantUmlText)
    {
        StringBufferConsolePrinter fakeConsole = new();
        Action a = () => TestHelper.RunSmRunnerForPlantUmlString(plantUmlText, propagateExceptions: false, consoleCapturer: fakeConsole);
        a.Should().Throw<FinishedWithFailureException>();
        string consoleOutput = fakeConsole.sb.ToString();
        consoleOutput.Should().Contain("StateSmith Runner - Finished with failure.");
        return consoleOutput;
    }

    /// <summary>
    /// Integration test for https://github.com/StateSmith/StateSmith/issues/283
    /// </summary>
    [Fact]
    public void ErrorReportingForBadInjectedCode_283()
    {
        const string InvalidActionCodeWithTwoFailReasons = $"printf(; via";

        var plantUmlText = """
            @startuml ExampleSm
            [*] --> MyStateWithBadCode
            @enduml
            """;

        // This test is more involved because it requires code injection
        StringBufferConsolePrinter fakeConsole = new();
        Action a = () => TestHelper.RunSmRunnerForPlantUmlString(plantUmlText, preRun: AddBadCode, propagateExceptions: false, consoleCapturer: fakeConsole);
        a.Should().Throw<FinishedWithFailureException>();

        string consoleOutput = fakeConsole.sb.ToString();
        consoleOutput.Should().Contain("Failed parsing: `printf(; via`");
        consoleOutput.Should().Contain("MyStateWithBadCode");
        consoleOutput.Should().Contain("""
            BehaviorValidationException: Failed parsing: `printf(; via`.
            Reason(s): no viable alternative at input ' via' at line 1 column 12. Offending symbol: `<EOF>`.
                       missing ')' at '<EOF>' at line 1 column 12. Offending symbol: `<EOF>`.
            Behavior:
                Owning vertex: ROOT.MyStateWithBadCode
                Target vertex: 
                Order: default
                Triggers: `enter`
                Guard: ``
                Action: `printf(; via`
                Via Entry: ``
                Via Exit : ``
                Diagram Id: ``
            """);

        void AddBadCode(SmRunner smRunner)
        {
            smRunner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, (sm) =>
            {
                sm.VisitTypeRecursively((State state) =>
                {
                    state.AddEnterAction(InvalidActionCodeWithTwoFailReasons); // clearly invalid code
                });
            });
        }
    }
}
