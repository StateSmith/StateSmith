using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmithTest.Processes.CComp;

namespace Spec.Spec2.Cpp;

/// <summary>
/// Required so that we only do the gcc compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public ICompilation compilation { get; private set; }

    public SharedCompilationFixture()
    {
        var action = (SmRunner runner) =>
        {
            runner.Settings.transpilerId = TranspilerId.Cpp;
            runner.AlgoOrTranspilerUpdated();
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action);

        this.compilation = new CCompilerMux().Compile(new CCompRequest()
        {
            WorkingDirectory = OutputDirectory,
            SourceFiles = ["../../lang-helpers/c/helper.c", "main.cpp", "Spec2Sm.cpp"],
            Flags = [
                // we disable `unused-function` warning because some states are intentionally unreachable
                CCompRequest.FlagId.IgnoreUnusedFunctions,
            ]
        });
    }

    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "Cpp/";

    public class MyGlueFile : IRenderConfig, IRenderConfigCpp
    {
        Spec2GenericVarExpansions spec2GenericVarExpansions = new();

        string IRenderConfigCpp.HFileIncludes => @"
                #include <stdint.h>
            ";

        string IRenderConfig.VariableDeclarations => @"
                uint8_t count;
            ";

        string IRenderConfig.AutoExpandedVars => "uint8_t auto_var_1;";
    }
}



