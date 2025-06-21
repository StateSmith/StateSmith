#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes;

namespace Spec.Spec2.Java;

/// <summary>
/// Required so that we only do the dotnet compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/java-spec2/";

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            // runner.Settings.transpilerId = TranspilerId.Java;
            // runner.AlgoOrTranspilerUpdated();
            //runner.Settings.outputGilCodeAlways = true;
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action, transpilerId: TranspilerId.Java);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "javac",
            Args = " MainClass.java Spec2SmBase.java Spec2Sm.java"
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    public class MyGlueFile : IRenderConfigJava
    {
        string IRenderConfig.FileTop => @"
            // any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated file
            ";

        string IRenderConfig.VariableDeclarations => @"
            public byte count;
            ";

        string IRenderConfig.AutoExpandedVars => @"
            public byte auto_var_1;
            ";

        string IRenderConfigJava.Extends => "Spec2SmBase";

        public class Expansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"MainClass.trace({message})"; // this isn't actually needed, but helps ensure expansions are working
        }
    }
}



