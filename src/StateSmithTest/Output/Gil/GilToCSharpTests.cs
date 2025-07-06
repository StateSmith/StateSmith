using FluentAssertions;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System.IO;
using System.Linq;
using Xunit;

namespace StateSmithTest.Output.Gil;

public class GilToCSharpTests
{
    /// <summary>
    /// Test for https://github.com/StateSmith/StateSmith/issues/122
    /// </summary>
    [Fact]
    public void DontRequireNameSpace()
    {
        SmRunner runner = SmRunner.Create(diagramPath: "CSharpNoNameSpaceExampleSm.drawio.svg", new DefaultCSharpRenderConfig(), transpilerId: TranspilerId.CSharp);
        runner.Settings.outputDirectory = Path.GetTempPath();
        runner.GetExperimentalAccess().Settings.propagateExceptions = true;
        runner.Settings.outputStateSmithVersionInfo = false;
        //runner.GetExperimentalAccess().Settings.dumpGilCodeOnError = true;
        runner.Run();
    }

    public class DefaultCSharpRenderConfig : IRenderConfigCSharp
    {
        //string IRenderConfigCSharp.NameSpace => "MySmNs";
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/231
    /// </summary>
    [Fact]
    public void TestNullableDisabled_231()
    {
        CapturingCodeFileWriter.Capture nonNullableFile = RenderAndCaptureOutput(new NullableDisableRenderConfig());
        CapturingCodeFileWriter.Capture regNullableFile = RenderAndCaptureOutput(new DefaultCSharpRenderConfig());

        regNullableFile.code.Should().Contain("#nullable enable");
        nonNullableFile.code.Should().NotContain("#nullable enable");

        regNullableFile.code.Should().Contain("currentStateExitHandler!(this)");
        nonNullableFile.code.Should().Contain("currentStateExitHandler(this)");

        regNullableFile.code.Should().Contain("private Func? ancestorEventHandler");
        nonNullableFile.code.Should().Contain("private Func ancestorEventHandler");
    }

    private static CapturingCodeFileWriter.Capture RenderAndCaptureOutput(IRenderConfig renderConfig)
    {
        var fakeFileSystem = TestHelper.CaptureSmRunnerFiles(diagramPath: "CSharpNoNameSpaceExampleSm.drawio.svg", renderConfig, TranspilerId.CSharp, AlgorithmId.Balanced1);
        var file = fakeFileSystem.GetCapturesForFileName("CSharpNoNameSpaceExampleSm.cs").Single();
        return file;
    }

    public class NullableDisableRenderConfig : IRenderConfigCSharp
    {
        bool IRenderConfigCSharp.UseNullable => false;
    }
}
