#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;

namespace StateSmith.SmGraph;

/// <summary>
/// Allows us to read settings from the diagram before the diagram is properly processed.
/// </summary>
public class PreDiagramSettingsReader
{
    RenderConfigAllVars renderConfigAllVars;
    RunnerSettings smRunnerSettings;
    IRenderConfig renderConfig;

    public PreDiagramSettingsReader(RenderConfigAllVars renderConfigAllVars, RunnerSettings smRunnerSettings, IRenderConfig renderConfig)
    {
        this.renderConfigAllVars = renderConfigAllVars;
        this.smRunnerSettings = smRunnerSettings;
        this.renderConfig = renderConfig;
    }

    /// <summary>
    /// This will do toml and standard (draw.io) diagram based render config processing.
    /// </summary>
    public void Process()
    {
        var di = DiServiceProvider.CreateDefault();
        SmRunner.SetupDiProvider(di, renderConfigAllVars, smRunnerSettings, renderConfig);

        di.Build();
        var inputSmBuilder = di.GetInstanceOf<InputSmBuilder>();

        SmRunnerInternal.SetupAndFindStateMachine(inputSmBuilder, smRunnerSettings);
        inputSmBuilder.FinishRunning();
    }
}
