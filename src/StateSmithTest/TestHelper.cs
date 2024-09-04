#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Output;
using System;
using System.IO;
using System.Reflection;

namespace StateSmithTest;

public class TestHelper
{
    public const string MinimalPlantUmlFsm = """
            @startuml RocketSm
            [*] --> c1
            @enduml
            """;

    public static string GetThisDir([System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        return Path.GetDirectoryName(callerFilePath) + "/";
    }

    public static (SmRunner, CapturingCodeFileWriter) CaptureSmRun(string diagramPath, IRenderConfig? renderConfig = null, TranspilerId transpilerId = TranspilerId.Default, AlgorithmId algorithmId = AlgorithmId.Default, IConsolePrinter? iConsolePrinter = null, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        SmRunner runner = new(diagramPath: diagramPath, renderConfig: renderConfig, transpilerId: transpilerId, algorithmId: algorithmId, callingFilePath: callerFilePath);
        runner.GetExperimentalAccess().Settings.propagateExceptions = true;
        var fakeFileSystem = new CapturingCodeFileWriter();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(fakeFileSystem);
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(iConsolePrinter ?? new DiscardingConsolePrinter());
        runner.Run();

        return (runner, fakeFileSystem);
    }

    public static CapturingCodeFileWriter CaptureSmRunnerFiles(string diagramPath, IRenderConfig? renderConfig = null, TranspilerId transpilerId = TranspilerId.Default, AlgorithmId algorithmId = AlgorithmId.Default, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        var (_, fakeFileSystem) = CaptureSmRun(diagramPath, renderConfig, transpilerId, algorithmId, callerFilePath: callerFilePath);
        return fakeFileSystem;
    }

    public static void CaptureRunSmRunnerForPlantUmlString(string? plantUmlText = null, IRenderConfig? renderConfig = null, ICodeFileWriter? codeFileWriter = null, Action<SmRunner>? postConstruct = null, Action<SmRunner>? preRun = null, bool propagateExceptions = true, string? fileName = null, IConsolePrinter? consoleCapturer = null, TranspilerId transpilerId = TranspilerId.Default, AlgorithmId algorithmId = AlgorithmId.Default)
    {
        string tempFilePath = WritePlantUmlTempFile(plantUmlText, fileName);

        try
        {
            SmRunner smRunner = new(diagramPath: tempFilePath, renderConfig: renderConfig, transpilerId: transpilerId, algorithmId: algorithmId);
            postConstruct?.Invoke(smRunner);
            smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(codeFileWriter ?? new DiscardingCodeFileWriter());
            smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(consoleCapturer ?? new DiscardingConsolePrinter());
            smRunner.Settings.propagateExceptions = propagateExceptions;
            preRun?.Invoke(smRunner);
            smRunner.Run();
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    private static string WritePlantUmlTempFile(string? plantUmlText = null, string? fileName = null)
    {
        var tempFilePath = Path.GetTempPath();
        tempFilePath += fileName ?? "statesmith_test" + Guid.NewGuid() + ".plantuml";
        File.WriteAllText(tempFilePath, plantUmlText ?? MinimalPlantUmlFsm);
        return tempFilePath;
    }

    public static void RunSmRunnerForPlantUmlString(string? plantUmlText = null, string? outputDir = null, TranspilerId transpilerId = TranspilerId.Default, AlgorithmId algorithmId = AlgorithmId.Default)
    {
        string tempFilePath = WritePlantUmlTempFile(plantUmlText);

        try
        {
            SmRunner smRunner = new(outputDirectory: outputDir, diagramPath: tempFilePath, transpilerId: transpilerId, algorithmId: algorithmId);
            smRunner.Settings.outputStateSmithVersionInfo = false; // too much git noise
            smRunner.Settings.propagateExceptions = true;
            smRunner.Run();
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    public static FieldInfo[] GetTypeFields<T>()
    {
        return typeof(T).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
    }

    public static MethodInfo[] GetTypeProperties<T>()
    {
        return typeof(T).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public);
    }

    public static void ExpectPropertyCount<T>(int expectedCount, string because = "")
    {
        GetTypeProperties<T>().Length.Should().Be(expectedCount, because: because);
    }

    public static void ExpectFieldCount<T>(int expectedCount, string because = "")
    {
        GetTypeFields<T>().Length.Should().Be(expectedCount, because: because);
    }
}
