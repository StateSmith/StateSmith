#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes;

namespace Spec.Spec2.TypeScript;

/// <summary>
/// Required so that we only do the nodejs compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/ts-spec2/";

    public SharedCompilationFixture()
    {
        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, transpilerId: TranspilerId.TypeScript);

        SimpleProcess process;

        // run `npm install` to get dependencies
        process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "npm",
            Args = "install"
        };
        process.IfWindowsWrapWithCmd();
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);

        // run `tsc` to compile TypeScript to JavaScript
        process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "tsc",
        };
        process.IfWindowsWrapWithCmd();
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    public class MyGlueFile : IRenderConfigTypeScript
    {
        string IRenderConfig.FileTop => """
            // any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated .h file
            import { trace, trace_guard } from "./printer";
            """;

        string IRenderConfig.VariableDeclarations => @"
                public count = 0;
            ";

        string IRenderConfig.AutoExpandedVars => @"
                public auto_var_1: number = 0;
            ";

        string IRenderConfigTypeScript.ClassCode => @"
                // some user class code
            ";

        public class CSharpExpansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"console.log({message})"; // this isn't actually needed, but helps ensure expansions are working
        }
    }
}



