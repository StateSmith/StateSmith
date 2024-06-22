using FluentAssertions;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Output;
using System;
using System.IO;
using System.Reflection;

#nullable enable

namespace StateSmithTest;

public class TestHelper
{
    public static string GetThisDir([System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        return Path.GetDirectoryName(callerFilePath) + "/";
    }

    public static (SmRunner, string) BuildSmRunnerForPlantUmlString(string plantUmlText, IRenderConfig? renderConfig, ICodeFileWriter? codeFileWriter = null)
    {
        var tempFilePath = Path.GetTempPath() + "statesmith.test" + Guid.NewGuid() + ".plantuml";
        File.WriteAllText(tempFilePath, plantUmlText);
        SmRunner smRunner = new(diagramPath: tempFilePath, renderConfig: renderConfig);
        smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(codeFileWriter ?? new DiscardingCodeFileWriter());
        InputSmBuilder inputSmBuilder = smRunner.GetExperimentalAccess().DiServiceProvider.GetInstanceOf<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.FindSingleStateMachine();
        smRunner.Settings.propagateExceptions = true;
        return (smRunner, tempFilePath);
    }

    public static (SmRunner, CapturingCodeFileWriter) CaptureSmRun(string diagramPath, IRenderConfig? renderConfig = null, TranspilerId transpilerId = TranspilerId.Default, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        SmRunner runner = new(diagramPath: diagramPath, renderConfig: renderConfig, transpilerId: transpilerId, callingFilePath: callerFilePath);
        runner.GetExperimentalAccess().Settings.propagateExceptions = true;
        var fakeFileSystem = new CapturingCodeFileWriter();
        runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(fakeFileSystem);
        runner.Run();

        return (runner, fakeFileSystem);
    }

    public static CapturingCodeFileWriter CaptureSmRunnerFiles(string diagramPath, IRenderConfig? renderConfig = null, TranspilerId transpilerId = TranspilerId.Default, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        var (_, fakeFileSystem) = CaptureSmRun(diagramPath, renderConfig, transpilerId, callerFilePath: callerFilePath);
        return fakeFileSystem;
    }

    public static void RunSmRunnerForPlantUmlString(string plantUmlText, IRenderConfig? renderConfig = null, ICodeFileWriter? codeFileWriter = null)
    {
        var (runner, tempFilePath) = BuildSmRunnerForPlantUmlString(plantUmlText, renderConfig, codeFileWriter);
        runner.Run();
        File.Delete(tempFilePath); // don't worry about deleting the file if exception is thrown. It is in temp folder.
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
