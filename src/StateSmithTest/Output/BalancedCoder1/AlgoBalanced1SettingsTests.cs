#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using Xunit;

namespace StateSmithTest.Output.BalancedCoder1;

// https://github.com/StateSmith/StateSmith/issues/181
// https://github.com/StateSmith/StateSmith/issues/535
// https://github.com/StateSmith/StateSmith/issues/538
public class AlgoBalanced1SettingsTests
{
    /// <summary>
    /// Captures what would normally be written to file to memory instead.
    /// </summary>
    readonly CapturingCodeFileWriter capturedFile = new();
    private SmRunner runner;

    private const string StateIdToString = "StateIdToString";
    private const string EventIdToString = "EventIdToString";
    private const string GetParentId = "GetParentId";
    private const string RootSubtree = "ROOT_Subtree";

    public AlgoBalanced1SettingsTests()
    {
        runner = new SmRunner(diagramPath: "ExBc1.drawio", transpilerId:TranspilerId.CSharp);
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(capturedFile);
        runner.Settings.propagateExceptions = true;
    }

    [Fact]
    public void DefaultBehavior()
    {
        runner.Run();

        capturedFile.LastCode.Should().Contain($"public static string {StateIdToString}(");
        capturedFile.LastCode.Should().Contain($"public static string {EventIdToString}(");
        capturedFile.LastCode.Should().Contain($"public static StateId {GetParentId}(");
        capturedFile.LastCode.Should().Contain($"{RootSubtree}");
    }

    [Fact]
    public void DisableAll()
    {
        runner.Settings.algoBalanced1.outputEventIdToStringFunction = false;
        runner.Settings.algoBalanced1.outputStateIdToStringFunction = false;
        runner.Settings.algoBalanced1.outputGetParentIdFunction = false;
        runner.Settings.algoBalanced1.outputSubtreeEndIds = false;
        runner.Run();

        capturedFile.LastCode.Should().NotContain($"public static string {StateIdToString}(");
        capturedFile.LastCode.Should().NotContain($"public static string {EventIdToString}(");
        capturedFile.LastCode.Should().NotContain($"public static StateId {GetParentId}(");
        capturedFile.LastCode.Should().NotContain($"{RootSubtree}");

    }

    [Fact]
    public void EnableAll()
    {
        runner.Settings.algoBalanced1.outputEventIdToStringFunction = true;
        runner.Settings.algoBalanced1.outputStateIdToStringFunction = true;
        runner.Settings.algoBalanced1.outputGetParentIdFunction = true;
        runner.Settings.algoBalanced1.outputSubtreeEndIds = true;
        runner.Run();

        capturedFile.LastCode.Should().Contain($"public static string {StateIdToString}(");
        capturedFile.LastCode.Should().Contain($"public static string {EventIdToString}(");
        capturedFile.LastCode.Should().Contain($"public static StateId {GetParentId}(");
        capturedFile.LastCode.Should().Contain($"{RootSubtree}");
    }
}
