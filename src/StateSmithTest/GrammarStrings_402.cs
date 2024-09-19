using FluentAssertions;
using StateSmith.Input.Antlr4;
using Xunit;

namespace StateSmithTest;

/// <summary>
/// See https://github.com/StateSmith/StateSmith/issues/282
/// See https://github.com/StateSmith/StateSmith/issues/402
/// Tests expect to find 3 functions: print, logger, format
/// </summary>
public class GrammarStrings_402
{
    [Fact]
    public void SingleQuoteStrings()
    {
        IntegrationTestActionCode("""print('some message'); logger(format('another message'));""");
        ParseExpectFunctions("""print('`some \n"message"`'); logger(format('another message\''));""");  // test strings inside strings
    }

    [Fact]
    public void BackTickStrings()
    {
        IntegrationTestActionCode("""print(`some message`); logger(format(`another message`));""");
        ParseExpectFunctions("""print(`'some "message"'`); logger(format(`another message\``));""");  // test strings inside strings
    }

    [Fact]
    public void CsVerbatimStrings()
    {
        IntegrationTestActionCode("""print(@"some message"); logger(format(@"another message"));""");
        //var s = @"some ""no_func()""";
        ParseExpectFunctions(""""print(@"some ""no_func()"""); logger(format(@"another message\"));"""");
    }

    [Fact]
    public void CppRawStrings()
    {
        IntegrationTestActionCode("""print(R"(some "message)"); logger(format(R"(another "message\")"));""");
    }

    [Fact]
    public void CppRawStrings2()
    {
        // these already work because grammar supports triple quoted strings
        IntegrationTestActionCode(""""print(R"""(some " message())"""); logger(format(R"""(another message)"""));"""");
    }

    [Fact]
    public void TripleSingleQuoteStrings()
    {
        IntegrationTestActionCode("""print('''some message'''); logger(format('''another message'''));""");
        ParseExpectFunctions("""print('''"some" '' ' message'''); logger(format('''another message\'''));""");
    }

    [Fact]
    public void TripleDoubleQuoteStrings()
    {
        IntegrationTestActionCode(""""print("""some message"""); logger(format("""another message"""));"""");
        ParseExpectFunctions(""""print("""'some ""message"" """); logger(format("""another message\"""));"""");
    }


    private static void IntegrationTestActionCode(string actionCode)
    {
        // below is problematic because old StateSmith would essentially see this: `print(string-content));` and it has unbalanced parentheses
        var plantUmlText = $$"""
            @startuml {fileName}
            [*] --> c1
            c1: enter / {{actionCode}}
            @enduml
            """;
        ProcessWithoutError(plantUmlText);
        ParseExpectFunctions(actionCode);
    }

    private static void ParseExpectFunctions(string actionCode)
    {
        ActionCodeInspector actionCodeInspector = new();
        actionCodeInspector.Parse(actionCode);
        actionCodeInspector.functionsCalled.Should().BeEquivalentTo(["print", "logger", "format"]);
    }

    private static void ProcessWithoutError(string plantUmlText)
    {
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText); // shouldn't throw an exception
    }
}
