using System;
using System.IO;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.SmGraph;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// This tiny class exists apart from SmRunner so that dependency injection can be used.
/// More SmRunner code will end up here.
/// </summary>
internal class SmRunnerInternal
{
    readonly InputSmBuilder inputSmBuilder;
    readonly RunnerSettings settings;
    readonly CodeGenRunner codeGenRunner;

    public SmRunnerInternal(InputSmBuilder inputSmBuilder, RunnerSettings settings, CodeGenRunner codeGenRunner)
    {
        this.inputSmBuilder = inputSmBuilder;
        this.settings = settings;
        this.codeGenRunner = codeGenRunner;
    }

    public void FinishRunning()
    {
        inputSmBuilder.FinishRunning();
        codeGenRunner.Run();
    }

    public StateMachine SetupAndFindStateMachine()
    {
        inputSmBuilder.ThrowIfNull().ConvertDiagramFileToSmVertices(settings.diagramFile);

        if (settings.stateMachineName != null)
        {
            inputSmBuilder.FindStateMachineByName(settings.stateMachineName);
        }
        else
        {
            inputSmBuilder.FindSingleStateMachine();
        }

        return inputSmBuilder.GetStateMachine();
    }
}



