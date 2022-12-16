using Spec.Spec1b;
using StateSmith.Input.Expansions;
using StateSmith.output;
using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;
using StateSmith.Runner;
using System;
using System.Diagnostics;
using Xunit;
using FluentAssertions;
using System.Runtime.InteropServices;
using StateSmithTest.Processes;

namespace Spec.Spec1b.C;

public class Spec1b_CTests
{
    public static string OutputDirectory => Spec1bFixture.Spec1Directory + "c/";

    public class MyGlueFile : IRenderConfigC
    {
        Spec1bGenericVarExpansions spec1bGenericVarExpansions = new();

        string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                #include <stdint.h>
            ");

        string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                #include ""../../lang-helpers/c/helper.h""
                #include ""printer.h""
            ");

        string IRenderConfigC.VariableDeclarations => StringUtils.DeIndentTrim(@"
                uint8_t count;
            ");
    }

    // see from https://en.wikipedia.org/wiki/UML_state_machine#Transition_execution_sequence
    [Fact]
    public void CodeGenAndTest()
    {
        Spec1bFixture.CompileAndRun(new MyGlueFile(), OutputDirectory);

        SimpleProcess process;

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "gcc ../../lang-helpers/c/helper.c main.c Spec1bSm.c"
        };
        BashRunner.RunCommand(process);

        process = new()
        {
            WorkingDirectory = OutputDirectory,
            CommandText = "./a.out"
        };
        BashRunner.RunCommand(process);

        // uncomment below line if you want to see the whole output
        // process.StdOutput.Should().Be("");

        var expected = StringUtils.DeIndentTrim(@"
            g() a(); b(); t(); c(); d(); e();
        ");
        // process.StdOutput.Should().Be("");

        Assert.Equal(expected, process.StdOutput.Trim());
    }
}



