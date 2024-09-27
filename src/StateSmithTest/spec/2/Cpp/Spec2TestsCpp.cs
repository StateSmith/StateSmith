using Xunit;
using StateSmithTest.spec2;
using StateSmithTest.Processes;
using StateSmith.Output;
using System.Text.RegularExpressions;

namespace Spec.Spec2.Cpp;

public class Spec2TestsCpp : Spec2Tests, IClassFixture<SharedCompilationFixture>
{
    private readonly SharedCompilationFixture _fixture;

    public Spec2TestsCpp(SharedCompilationFixture sharedCompilationFixture)
    {
        _fixture = sharedCompilationFixture;
    }

    public override string RunProcess(string testEvents)
    {
        SimpleProcess process = _fixture.compilation.RunExecutable(runArgs: testEvents);

        string output = process.StdOutput;
        output = StringUtils.RemoveEverythingBefore(output, "\nIGNORE_OUTPUT_BEFORE_THIS\n").Trim();
        output = Regex.Replace(output, @"[\s\S]*\nCLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\n[\s\S]*?\n\n", "").Trim();
        //output = output.Replace("sm->vars", "sm.vars");

        output = PreProcessOutput(output);

        return output;
    }
}

