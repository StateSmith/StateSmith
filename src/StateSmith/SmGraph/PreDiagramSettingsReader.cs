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

        ModifyTransformationPipeline(inputSmBuilder);

        SmRunnerInternal.SetupAndFindStateMachine(inputSmBuilder, smRunnerSettings);
        inputSmBuilder.FinishRunning();
    }

    private static void ModifyTransformationPipeline(InputSmBuilder inputSmBuilder)
    {
        // Remove everything not needed for diagram settings reading.
        // We don't actually want to validate the diagram, just read the settings.
        // Why? Because it is slower and also we don't want to mess up designs that require special transformers.
        // If a user adds special transformers, they won't be added here as this is a brand new SmRunner and DI setup.
        // https://github.com/StateSmith/StateSmith/issues/349

        inputSmBuilder.transformer.RemoveAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_SupportRenderConfigVerticesAndRemove);
        
        // ensure that remove above didn't remove the toml config processor
        if (!inputSmBuilder.transformer.HasMatch(StandardSmTransformer.TransformationId.Standard_TomlConfig))
        {
            throw new System.InvalidOperationException("Programming error. Standard_TomlConfig must be present.");
        }

        if (inputSmBuilder.transformer.transformationPipeline.Count != 3)
        {
            throw new System.InvalidOperationException("Programming error. Expected only 3 steps in the pipeline.");
        }
    }
}
