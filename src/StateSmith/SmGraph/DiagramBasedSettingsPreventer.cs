#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System;

namespace StateSmith.SmGraph;

/// <summary>
/// Sometimes we need to prevent the diagram from setting the settings.
/// Useb by the simulator and also to prevent the settings from being applied twice.
/// </summary>
public class DiagramBasedSettingsPreventer
{
    public static void Process(SmTransformer transformer, Action<RenderConfigAllVars, RunnerSettings>? action = null)
    {
        transformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_TomlConfig, (sm) =>
        {
            // create temp settings/config objects that may get modified by special diagram nodes
            RenderConfigAllVars tempRenderConfigAllVars = new();
            RunnerSettings tempSmRunnerSettings = new();
            var tomlConfigVerticesProcessor = new TomlConfigVerticesProcessor(tempRenderConfigAllVars, tempSmRunnerSettings);
            tomlConfigVerticesProcessor.Process(sm);
            var renderConfigVerticesProcessor = new RenderConfigVerticesProcessor(tempRenderConfigAllVars, sm);
            renderConfigVerticesProcessor.Process();

            action?.Invoke(tempRenderConfigAllVars, tempSmRunnerSettings);
        });

        // these transformations are no longer needed for the simulation
        transformer.Remove(StandardSmTransformer.TransformationId.Standard_TomlConfig);
        transformer.Remove(StandardSmTransformer.TransformationId.Standard_SupportRenderConfigVerticesAndRemove);
    }
}
