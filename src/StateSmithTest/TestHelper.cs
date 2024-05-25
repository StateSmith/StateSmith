using StateSmith.Output;
using StateSmith.Runner;
using StateSmithTest.Output;
using System.IO;

#nullable enable

namespace StateSmithTest;

public class TestHelper
{
    public static string GetThisDir([System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        return Path.GetDirectoryName(callerFilePath) + "/";
    }

    public static SmRunner BuildSmRunnerForPlantUmlString(string plantUmlText)
    {
        SmRunner smRunner = new(diagramPath: "no-actual-file.plantuml");
        smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(new DiscardingCodeFileWriter());
        InputSmBuilder inputSmBuilder = smRunner.GetExperimentalAccess().DiServiceProvider.GetInstanceOf<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.FindSingleStateMachine();
        smRunner.Settings.propagateExceptions = true;
        return smRunner;
    }

    public static void RunSmRunnerForPlantUmlString(string plantUmlText)
    {
        BuildSmRunnerForPlantUmlString(plantUmlText).Run();
    }
}
