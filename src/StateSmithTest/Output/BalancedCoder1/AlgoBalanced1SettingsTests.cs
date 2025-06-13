#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace StateSmithTest.Output.BalancedCoder1;

public class AlgoBalanced1SettingsTests
{
    /// <summary>
    /// Captures what would normally be written to file to memory instead.
    /// </summary>
    readonly CapturingCodeFileWriter capturedFile = new();

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void NormalBehaviorHasToStringFunctions()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId:TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Run();

        capturedFile.LastCode.Should().Contain("public static string StateIdToString(");
        capturedFile.LastCode.Should().Contain("public static string EventIdToString(");
    }

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void RemoveEventIdToString()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Settings.algoBalanced1.outputEventIdToStringFunction = false; // Here's the setting you want
        runner.Run();

        capturedFile.LastCode.Should().Contain("public static string StateIdToString");
        capturedFile.LastCode.Should().NotContain("public static string EventIdToString(");
    }

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void RemoveStateIdToString()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Settings.algoBalanced1.outputStateIdToStringFunction = false; // Here's the setting you want
        runner.Run();

        capturedFile.LastCode.Should().NotContain("public static string StateIdToString");
        capturedFile.LastCode.Should().Contain("public static string EventIdToString(");
    }

    [Fact]
    public void Setting_outputEventIdIsValidFunction_473()
    {
        var plantUml = """"
            @startuml MySm
            [*] -> S1
            S1 -> S2: EV1

            /'! $CONFIG : toml
                SmRunnerSettings.algoBalanced1.outputEventIdIsValidFunction = false
            '/
            @enduml
            """";

        var disabledFiles = TestHelper.CaptureSmRunnerFilesForPlantUmlString(plantUml, transpilerId: StateSmith.Runner.TranspilerId.C99);

        // remove disabled setting and test with it default enabled
        plantUml = Regex.Replace(plantUml, @"SmRunnerSettings.*", "");
        var enabledFiles = TestHelper.CaptureSmRunnerFilesForPlantUmlString(plantUml, transpilerId: StateSmith.Runner.TranspilerId.C99);

        const string FUNCTION_NAME = "MySm_is_event_id_valid";
        disabledFiles.GetSoleCaptureWithName("MySm.h").code.Should().NotContain(FUNCTION_NAME);
        disabledFiles.GetSoleCaptureWithName("MySm.c").code.Should().NotContain(FUNCTION_NAME);

        enabledFiles.GetSoleCaptureWithName("MySm.h").code.Should().Contain(FUNCTION_NAME);
        enabledFiles.GetSoleCaptureWithName("MySm.c").code.Should().Contain(FUNCTION_NAME);
    }

    private static void SetupForUnitTest(CapturingCodeFileWriter capturedFile, SmRunner runner)
    {
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(capturedFile);
        runner.Settings.propagateExceptions = true;
    }
}
