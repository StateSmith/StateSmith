using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmithTest.Processes.CComp;

namespace Spec.Spec2.C;

/// <summary>
/// Required so that we only do the gcc compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public ICompilation compilation { get; private set; }

    public SharedCompilationFixture()
    {
        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory);

        this.compilation = new CCompilerMux().Compile(new CCompRequest()
        {
            WorkingDirectory = OutputDirectory,
            SourceFiles = ["../../lang-helpers/c/helper.c", "main.c", "Spec2Sm.c"],
            Flags = [
                // we disable `unused-function` warning because some states are intentionally unreachable
                CCompRequest.FlagId.IgnoreUnusedFunctions,
            ]
        });
    }

    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "c/";

    public class MyGlueFile : IRenderConfig, IRenderConfigC
    {
        Spec2GenericVarExpansions spec2GenericVarExpansions = new();

        string IRenderConfigC.HFileIncludes => @"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ";

        string IRenderConfigC.CFileIncludes => @"
                #include ""../../lang-helpers/c/helper.h""
            ";

        string IRenderConfig.VariableDeclarations => @"
                uint8_t count;
            ";

        string IRenderConfig.AutoExpandedVars => "uint8_t auto_var_1;";
    }
}



