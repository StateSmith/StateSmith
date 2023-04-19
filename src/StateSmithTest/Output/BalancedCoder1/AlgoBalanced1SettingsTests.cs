#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
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

        capturedFile.code.Should().Contain("public static string StateIdToString(");
        capturedFile.code.Should().Contain("public static string EventIdToString(");
    }

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void RemoveEventIdToString()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Settings.algoBalanced1Settings.generateEventIdToStringFunction = false; // Here's the setting you want
        runner.Run();

        capturedFile.code.Should().Contain("public static string StateIdToString");
        capturedFile.code.Should().NotContain("public static string EventIdToString(");
    }

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void RemoveStateIdToString()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Settings.algoBalanced1Settings.generateStateIdToStringFunction = false; // Here's the setting you want
        runner.Run();

        capturedFile.code.Should().NotContain("public static string StateIdToString");
        capturedFile.code.Should().Contain("public static string EventIdToString(");
    }


    // https://github.com/StateSmith/StateSmith/issues/141
    [Fact]
    public void NormalDispatchHasVoidReturn()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Run();

        capturedFile.code.Should().Contain("public void DispatchEvent(");
    }

    // https://github.com/StateSmith/StateSmith/issues/141
    [Fact]
    public void DispatchEventReturnAndValidation()
    {
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp);
        SetupForUnitTest(capturedFile, runner);
        runner.Settings.outputGilCodeAlways = true;
        runner.Run();

        capturedFile.code.Should().NotContain("public void DispatchEvent(");
        capturedFile.code.Should().Contain("public ResultId DispatchEvent(");
    }

    private static void SetupForUnitTest(CapturingCodeFileWriter capturedFile, SmRunner runner)
    {
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(capturedFile);
        runner.Settings.propagateExceptions = true;
        runner.Settings.dumpGilCodeOnError = true;
    }
}
