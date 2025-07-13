using Xunit;
using StateSmithTest.spec2;
using StateSmithTest.Processes;
using StateSmith.Output;
using System.Text.RegularExpressions;

namespace Spec.Spec2.Python;

public class Spec2TestsPython : Spec2Tests, IClassFixture<SharedCompilationFixture>
{
    override public string True => "True";
    override public string False => "False";
    override public string PostInc => " += 1";
    override public string SemiColon => "";

    public override string RunProcess(string testEvents)
    {
        SimpleProcess process = new()
        {
            WorkingDirectory = SharedCompilationFixture.OutputDirectory,
            ProgramPath = "python3",
            Args = $" MainClass.py {testEvents}"
        };
        process.Run(SimpleProcess.DefaultLongTimeoutMs);

        string output = process.StdOutput;
        output = StringUtils.RemoveEverythingBefore(output, "\nIGNORE_OUTPUT_BEFORE_THIS\n").Trim();
        output = Regex.Replace(output, @"[\s\S]*\nCLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\n[\s\S]*?\n\n", "").Trim();

        output = PreProcessOutput(output);

        return output;
    }
}
