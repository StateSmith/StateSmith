#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using Xunit;
using Microsoft.Extensions.DependencyInjection;


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
        var spBuilder = DefaultServiceProviderBuilder.CreateDefault((services) =>
        {
            services.AddSingleton<ICodeFileWriter>(capturedFile);
        });
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId:TranspilerId.CSharp, serviceProviderBuilder: spBuilder);
        runner.Settings.propagateExceptions = true;
        runner.Run();

        capturedFile.LastCode.Should().Contain("public static string StateIdToString(");
        capturedFile.LastCode.Should().Contain("public static string EventIdToString(");
    }

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void RemoveEventIdToString()
    {
        var spBuilder = DefaultServiceProviderBuilder.CreateDefault((services) =>
        {
            services.AddSingleton<ICodeFileWriter>(capturedFile);
        });
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp, serviceProviderBuilder: spBuilder);
        runner.Settings.propagateExceptions = true;
        runner.Settings.algoBalanced1.outputEventIdToStringFunction = false; // Here's the setting you want
        runner.Run();

        capturedFile.LastCode.Should().Contain("public static string StateIdToString");
        capturedFile.LastCode.Should().NotContain("public static string EventIdToString(");
    }

    // https://github.com/StateSmith/StateSmith/issues/181
    [Fact]
    public void RemoveStateIdToString()
    {
        var spBuilder = DefaultServiceProviderBuilder.CreateDefault((services) =>
        {
            services.AddSingleton<ICodeFileWriter>(capturedFile);
        });
        SmRunner runner = new(diagramPath: "ExBc1.drawio", transpilerId: TranspilerId.CSharp, serviceProviderBuilder: spBuilder);
        runner.Settings.propagateExceptions = true;
        runner.Settings.algoBalanced1.outputStateIdToStringFunction = false; // Here's the setting you want
        runner.Run();

        capturedFile.LastCode.Should().NotContain("public static string StateIdToString");
        capturedFile.LastCode.Should().Contain("public static string EventIdToString(");
    }

}
