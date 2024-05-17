using FluentAssertions;
using StateSmith.Cli.GenTestScaffold;
using StateSmith.Cli.GenTestScaffold.Template;
using Xunit;

namespace StateSmith.CliTest.GenTestScaffold.Template;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/266
/// </summary>
public class JsJestTest
{
    private JsJest generator = new();

    [Fact]
    public void SuggestFileName_Test()
    {
        GeneratorData data = new()
        {
            StateMachineName = "RocketSm",
            FirstTestStateName = "WARM_UP",
            FirstTestEventName = "PRIME",
        };

        generator.SuggestFileName(data).Should().Be("RocketSm.jest.test.js");
    }

    [Fact]
    public void GenerateFileContent_TestNormal()
    {
        GeneratorData data = new()
        {
            StateMachineName = "RocketSm",
            FirstTestStateName = "WARM_UP",
            FirstTestEventName = "PRIME",
        };

        string content = generator.GenerateFileContent(data);

        content.Should().NotContain("{{SmName}}");
        content.Should().NotContain("{{FirstEvent}}");
        content.Should().NotContain("{{FirstState}}");

        string[] expectedLines = [
            "import { RocketSm } from './RocketSm.js';",
            "expect(sm.stateId).toBe(RocketSm.StateId.WARM_UP);",
            "sm.dispatchEvent(RocketSm.EventId.PRIME);",
            "const sm = new RocketSm();",
            "expect(sm.stateId).toBe(RocketSm.StateId.WARM_UP);",
        ];

        foreach (var line in expectedLines)
        {
            content.Should().Contain(line);
        }
    }

    /// <summary>
    /// This may happen if initial state has multiple transitions.
    /// </summary>
    [Fact]
    public void GenerateFileContent_TestMissingFirstState()
    {
        GeneratorData data = new()
        {
            StateMachineName = "RocketSm",
            FirstTestStateName = null,
            FirstTestEventName = "PRIME",
        };

        string content = generator.GenerateFileContent(data);

        string[] expectedLines = [
            "expect(sm.stateId).toBe(RocketSm.StateId.YOUR_FIRST_STATE);",
            "expect(sm.stateId).toBe(RocketSm.StateId.YOUR_FIRST_STATE);",
        ];

        foreach (var line in expectedLines)
        {
            content.Should().Contain(line);
        }
    }
}
