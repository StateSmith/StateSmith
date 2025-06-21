#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes;

namespace Spec.Spec2.CSharp;

/// <summary>
/// Required so that we only do the dotnet compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/csharp-spec2/";

    public SharedCompilationFixture()
    {
        // Delete the output directory to ensure a clean build.
        // This is important because we use a path based on dotnet version to run faster.
        // If we don't delete the output directory, we could accidentally use an old build.
        string binDebugPath = OutputDirectory + "bin/Debug";
        if (System.IO.Directory.Exists(binDebugPath))
        {
            System.IO.Directory.Delete(binDebugPath, recursive: true);
        }

        var action = (SmRunner runner) =>
        {
            // runner.Settings.transpilerId = TranspilerId.CSharp;
            // runner.AlgoOrTranspilerUpdated();
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action, transpilerId: TranspilerId.CSharp);

        SimpleProcess process = new()
        {
            WorkingDirectory = OutputDirectory,
            ProgramPath = "dotnet",
            Args = "build --verbosity quiet"
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    public class MyGlueFile : IRenderConfigCSharp
    {
        string IRenderConfig.FileTop => @"
                // any text you put in IRenderConfig.FileTop (like this comment) will be written to the generated .h file
            ";

        string IRenderConfig.VariableDeclarations => @"
                public byte count;
            ";

        string IRenderConfig.AutoExpandedVars => @"
                public byte auto_var_1;
            ";

        bool IRenderConfigCSharp.UseNullable => true;

        string IRenderConfigCSharp.NameSpace => "Csharp.Spec2smTests";

        string IRenderConfigCSharp.Usings => @"
                using StateSmithTest.spec._2.CSharp; // to get access to MainClass
            ";

        string IRenderConfigCSharp.ClassCode => @"
                // trace() implemented in base class
                // trace_guard() implemented in partial class
            ";

        string IRenderConfigCSharp.BaseList => "Spec2SmBase";

        public class CSharpExpansions : Spec2GenericVarExpansions
        {
            public override string trace(string message) => $"MainClass.Trace({message})"; // this isn't actually needed, but helps ensure expansions are working
        }
    }
}



