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
            // runner.Settings.transpilerId = TranspilerId.Cpp;
            // runner.AlgoOrTranspilerUpdated();
        };

        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory, action: action, transpilerId: TranspilerId.Cpp);

        // compile C code first
        new CCompilerMux().Compile(new CCompRequest()
        {
            IsCpp = false,
            WorkingDirectory = OutputDirectory,
            SourceFiles = ["../../lang-helpers/c/helper.c", "-o", "helper.o", "-c"], // later parts aren't files, but this is good enough for now
            IncludePaths = ["../../lang-helpers/c"],
            Flags = [
                // we disable `unused-function` warning because some states are intentionally unreachable
                CCompRequest.FlagId.IgnoreUnusedFunctions,
            ]
        });

        // compile C++ and link with C object files
        this.compilation = new CCompilerMux().Compile(new CCompRequest()
        {
            IsCpp = true,
            WorkingDirectory = OutputDirectory,
            SourceFiles = ["helper.o", "main.cpp", "Spec2Sm.cpp"],
            IncludePaths = ["../../lang-helpers/c"],
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

        string IRenderConfigCpp.HFileIncludes => """
            #include "Spec2SmBase.hpp"
            """;

        string IRenderConfig.VariableDeclarations => @"
                uint8_t count;
            ";

        string IRenderConfigCpp.ClassCode => """
            public:
                void user_class_code_example()
                {
                    // your code here
                }

            private:
                void private_user_class_code_example()
                {
                    // your code here
                }
            """;

        string IRenderConfigCpp.BaseClassCode => "public Spec2SmBase";

        string IRenderConfig.AutoExpandedVars => "uint8_t auto_var_1;";

        string IRenderConfigCpp.NameSpace => "Spec2";
    }
}



