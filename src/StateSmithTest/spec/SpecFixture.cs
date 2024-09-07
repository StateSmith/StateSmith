using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System;

#nullable enable

/*
 * This file is intended to provide language agnostic helpers and expansions for all specification tests
 */

// spell-checker: ignore modder

namespace Spec;

public class SpecFixture
{
    public static string SpecInputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "../../../spec/";

    public static void CompileAndRun(IRenderConfig renderConfig, string diagramFile, string srcDirectory, bool useTracingModder = true, Action<SmRunner>? smRunnerAction = null)
    {
        RunnerSettings settings = new(diagramFile: diagramFile, outputDirectory: srcDirectory);
        settings.outputStateSmithVersionInfo = false; // too much noise in repo

        settings.algorithmId = Environment.GetEnvironmentVariable("STATESMITH_TEST_ALGORITHM_ID") switch
        {
            nameof(AlgorithmId.Balanced1) => AlgorithmId.Balanced1,
            nameof(AlgorithmId.Balanced2) => AlgorithmId.Balanced2,
            _ => AlgorithmId.Default,
        };

        //settings.outputGilCodeAlways = true;
        SmRunner runner = new(settings, renderConfig);

        smRunnerAction?.Invoke(runner);

        settings.propagateExceptions = true;
        settings.dumpGilCodeOnError = true;

        if (useTracingModder)
        {
            runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1,
                new TransformationStep(id: nameof(TracingModder), action: (sm) => new TracingModder().AddTracingBehaviors(sm)));
        }
        runner.Run();
    }
}

public class SpecGenericVarExpansions : UserExpansionScriptBase
{
#pragma warning disable IDE1006 // Naming Styles
    public virtual string trace(string message) => $"trace({message})";
    public virtual string trace_guard(string message, string guardCode) => $"trace_guard({message}, {guardCode})";

    public string clear_output() => $"{trace("\"IGNORE_OUTPUT_BEFORE_THIS\"")};";
    public string clear_dispatch_output() => $"{trace("\"CLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\"")};"; // TODO - remove extra comma
#pragma warning restore IDE1006 // Naming Styles
}

