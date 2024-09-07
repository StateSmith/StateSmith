#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes;

namespace Spec.Spec2.JavaScript;

/// <summary>
/// Required so that we only do the nodejs compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/js-spec2/";

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.Settings.transpilerId = TranspilerId.JavaScript;
            runner.AlgoOrTranspilerUpdated();
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "node",
            Args = " --check Spec2Sm.js"
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    public class MyGlueFile : IRenderConfigJavaScript
    {
        string IRenderConfig.FileTop => """
            "use strict";    
            // any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated .h file
            import { trace, trace_guard } from "./printer.js";
            """;

        bool IRenderConfigJavaScript.UseExportOnClass => true;

        string IRenderConfigJavaScript.PrivatePrefix => "_";

        string IRenderConfig.VariableDeclarations => @"
                count: 0,
            ";

        string IRenderConfig.AutoExpandedVars => @"
                auto_var_1: 0,
            ";

        string IRenderConfigJavaScript.ClassCode => @"
                // some class code
            ";

        public class CSharpExpansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"console.log({message})"; // this isn't actually needed, but helps ensure expansions are working
        }
    }
}



