#nullable enable

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
    InputSmBuilder inputSmBuilder;

    public PreDiagramSettingsReader(RunnerSettings smRunnerSettings, InputSmBuilder inputSmBuilder)
    {
        this.smRunnerSettings = smRunnerSettings;
        this.inputSmBuilder = inputSmBuilder;
    }

    /// <summary>
    /// This will do toml and standard (draw.io) diagram based render config processing.
    /// </summary>
    public void Process()
    {
        SmRunnerInternal.SetupAndFindStateMachine(inputSmBuilder, smRunnerSettings); // TODO do we need this anymore?
        inputSmBuilder.FinishRunning();
    }
}
