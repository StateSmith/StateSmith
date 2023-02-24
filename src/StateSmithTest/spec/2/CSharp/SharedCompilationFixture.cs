using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Gil.CSharp;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes;
using System.Text;

#nullable enable

namespace Spec.Spec2.CSharp;

/// <summary>
/// Required so that we only do the gcc compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "../../../test-misc/csharp-spec2/";

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IGilTranspiler, GilToCSharp>();
            runner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "dotnet build --verbosity quiet"
        };
        BashRunner.RunCommand(process, timeoutMs: 8000);
    }

    public class MyGlueFile : IRenderConfigCSharp
    {
        string IRenderConfig.FileTop => @"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ";

        string IRenderConfig.VariableDeclarations => @"
                public byte count;
            ";

        string IRenderConfig.AutoExpandedVars => @"
                public byte auto_var_1;
            ";

        string IRenderConfigCSharp.NameSpace => @"
                Csharp.Spec2smTests
            ";

        string IRenderConfigCSharp.Usings => @"
                using StateSmithTest.spec._2.CSharp; // to get access to MainClass
            ";

        string IRenderConfigCSharp.ClassCode => @"
                private void trace(string message) => MainClass.trace(message);
                private bool trace_guard(string message, bool b) => MainClass.trace_guard(message, b);
            ";

        public class CSharpExpansions : Spec2GenericVarExpansions
        {
            public string sb => AutoVarName();
            public override string trace(string message) => $"trace({message})";
            public override string trace_guard(string message, string guardCode) => $"trace_guard({message}, {guardCode})";
        }
    }
}



