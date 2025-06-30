#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Output.UserConfig.AutoVars;
using StateSmith.Runner;
using Microsoft.Extensions.DependencyInjection;

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
        // TODO use the IServiceProvider from SmRunnerInternal once it's fully pre-constructed
        // TODO actually, it's not needed at all, you just need to inject the InputSmBuilder
        var di = IConfigServiceProviderBuilder.CreateDefault()
            .WithRenderConfig(renderConfigAllVars, renderConfig)
            .WithRunnerSettings(smRunnerSettings)
            .Build();

        var inputSmBuilder = di.GetRequiredService<PreDiagramSettingsInputSmBuilder>();

        SmRunnerInternal.SetupAndFindStateMachine(inputSmBuilder, smRunnerSettings);
        inputSmBuilder.FinishRunning();
    }
}
