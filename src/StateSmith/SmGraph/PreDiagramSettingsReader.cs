#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Output.UserConfig.AutoVars;
using StateSmith.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace StateSmith.SmGraph;

/// <summary>
/// Allows us to read settings from the diagram before the diagram is properly processed.
/// </summary>
public class PreDiagramSettingsReader
{
    RunnerSettings smRunnerSettings;
    IServiceProvider sp;

    public PreDiagramSettingsReader(RunnerSettings smRunnerSettings, IServiceProvider sp)
    {
        this.smRunnerSettings = smRunnerSettings;
        this.sp = sp;
    }

    /// <summary>
    /// This will do toml and standard (draw.io) diagram based render config processing.
    /// </summary>
    public void Process()
    {
        // TODO inject this
        var inputSmBuilder = sp.GetRequiredService<InputSmBuilder>();

        SmRunnerInternal.SetupAndFindStateMachine(inputSmBuilder, smRunnerSettings); // TODO do we need this anymore?
        inputSmBuilder.FinishRunning();
    }
}
