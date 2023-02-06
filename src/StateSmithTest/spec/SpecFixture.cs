using StateSmith.Common;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;

/*
 * This file is intended to provide language agnostic helpers and expansions for all specification tests
 */

// spell-checker: ignore modder

namespace Spec;

public class SpecFixture
{
    public static string SpecInputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "../../../spec/";

    public static void CompileAndRun(IRenderConfigC renderConfigC, string diagramFile, string srcDirectory, bool useTracingModder = true)
    {
        RunnerSettings settings = new(renderConfigC, diagramFile: diagramFile, outputDirectory: srcDirectory);
        SmRunner runner = new(settings);
        settings.propagateExceptions = true;
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
    public string clear_output() => "trace(\"IGNORE_OUTPUT_BEFORE_THIS\");";
    public string clear_dispatch_output() => "trace(\"CLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\");";
#pragma warning restore IDE1006 // Naming Styles
}

