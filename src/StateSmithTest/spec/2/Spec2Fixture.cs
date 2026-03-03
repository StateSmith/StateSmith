using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System;

#nullable enable

/*
 * This file is intended to provide language agnostic helpers and expansions
 */

namespace Spec.Spec2;

public class Spec2GenericVarExpansions : SpecGenericVarExpansions
{
    #pragma warning disable IDE1006 // Naming Styles
    public string count => AutoVarName();

    public string trace_meta()
    {
        var quotedMessage = $"""
        "META: State: {CurrentNamedVertex.Name}, trigger: {CurrentTrigger}, behavior vertex: {Vertex.Describe(CurrentBehavior.OwningVertex)}"
        """;

        return trace(quotedMessage);
    }

#pragma warning restore IDE1006 // Naming Styles
}

public class Spec2Fixture : SpecFixture
{
    public static string Spec2Directory => SpecInputDirectoryPath + "2/";

    public static void CompileAndRun(IRenderConfig renderConfig, string outputDir, Action compilationAction, Action<SmRunner>? preRunAction = null, string semiColon = ";", string trueString = "true")
    {
        var diagramFile = Spec2Directory + "Spec2Sm.graphml";

        // Compile once without tracing first to ensure normally generated code is valid (compiles) before being modified.
        // Important for Java, but a good practice for all. https://github.com/StateSmith/StateSmith/issues/507
        CompileAndRun(renderConfig, diagramFile: diagramFile, srcDirectory: outputDir, useTracingModder:false, preRunAction: preRunAction, semiColon: semiColon, trueString: trueString);
        compilationAction();

        CompileAndRun(renderConfig, diagramFile: diagramFile, srcDirectory: outputDir, useTracingModder:true, preRunAction: preRunAction, semiColon: semiColon, trueString: trueString);
        compilationAction?.Invoke();
    }
}

