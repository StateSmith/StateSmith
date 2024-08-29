using StateSmith.Output;
using StateSmith.Output.UserConfig;
using Xunit;
using StateSmithTest.Processes.CComp;

namespace Spec.Spec1b.C;

public class Spec1b_CTests
{
    public static string OutputDirectory => Spec1bFixture.Spec1Directory + "c/";

    public class MyGlueFile : IRenderConfig, IRenderConfigC
    {
        Spec1bGenericVarExpansions spec1bGenericVarExpansions = new();

        string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // any text you put in IRenderConfigC.HFileIncludes (like this comment) will be written to the generated .h file
            ");

        string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""../../lang-helpers/c/helper.h""
                #include ""printer.h""
            ");

        string IRenderConfig.VariableDeclarations => StringUtils.DeIndentTrim(@"
                uint8_t count;
            ");

        /// <summary>
        /// https://github.com/StateSmith/StateSmith/issues/112
        /// </summary>
        string IRenderConfigC.IncludeGuardLabel => "SPEC1B_CUSTOM_INCLUDE_GUARD_H";
    }

    // see from https://en.wikipedia.org/wiki/UML_state_machine#Transition_execution_sequence
    [Fact]
    public void CodeGenAndTest()
    {
        Spec1bFixture.CompileAndRun(new MyGlueFile(), OutputDirectory);

        ICompilation compilation = new CCompilerMux().Compile(new CCompRequest(){
            WorkingDirectory = OutputDirectory,
            SourceFiles = ["../../lang-helpers/c/helper.c", "main.c", "Spec1bSm.c"],
        });

        var process = compilation.RunExecutable();

        var expected = StringUtils.DeIndentTrim(@"
            g() a(); b(); t(); c(); d(); e();
        ");

        Assert.Equal(expected, process.StdOutput.Trim());
    }
}



