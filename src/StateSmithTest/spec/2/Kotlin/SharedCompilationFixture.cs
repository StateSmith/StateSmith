#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes;

namespace Spec.Spec2.Kotlin;

/// <summary>
/// Required so that we only do the dotnet compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/kt-spec2/";

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.Settings.transpilerId = TranspilerId.Kotlin;
            runner.AlgoOrTranspilerUpdated();
            //runner.Settings.outputGilCodeAlways = true;
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "kotlinc.bat",
            Args = " MainClass.kt Spec2SmBase.kt Spec2Sm.kt -include-runtime -d test.jar"
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    public class MyGlueFile : IRenderConfigKotlin
    {
        string IRenderConfig.FileTop => @"
            @file:Suppress(""ALL"")
            // any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated file
            ";

        string IRenderConfig.VariableDeclarations => @"
            var count = 0
            ";

        string IRenderConfig.AutoExpandedVars => @"
            var auto_var_1 = 0
            ";

        string IRenderConfigKotlin.Extends => "Spec2SmBase";

        public class Expansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"MainClass.trace({message.Replace("$", "\\$")})";

            public override string trace_guard(string message, string guardCode) => $"trace_guard({message.Replace("$", "\\$")}, {guardCode})";
        }
    }
}



