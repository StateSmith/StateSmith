using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmithTest.Processes;

namespace Spec.Spec2.C;

/// <summary>
/// Required so that we only do the gcc compilation once to avoid concurrency conflicts.
/// This will only be constructed once and shared amongst any tests that need it.
/// </summary>
public class SharedCompilationFixture
{
    public SharedCompilationFixture()
    {
        Spec2Fixture.CompileAndRun(new MyGlueFile(), OutputDirectory);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "gcc -Wall -Wno-unused-function ../../lang-helpers/c/helper.c main.c Spec2Sm.c" // we disable `unused-function` warning because some states are intentionally unreachable
        };
        BashRunner.RunCommand(process);
    }

    public static string OutputDirectory => Spec2Fixture.Spec2Directory + "c/";

    public class MyGlueFile : IRenderConfigC
    {
        Spec2GenericVarExpansions spec2GenericVarExpansions = new();

        string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ");

        string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""../../lang-helpers/c/helper.h""
            ");

        string IRenderConfigC.VariableDeclarations => StringUtils.DeIndentTrim(@"
                uint8_t count;
            ");
    }
}



