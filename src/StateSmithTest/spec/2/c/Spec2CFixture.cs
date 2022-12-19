using StateSmith.output;
using Xunit;
using StateSmithTest.Processes;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Spec.Spec2.C;

public class Spec2CFixture : IClassFixture<SharedCompilationFixture>
{
    public string Run(string initialEventToSelectTest, string testEvents)
    {
        SimpleProcess process = new()
        {
            WorkingDirectory = SharedCompilationFixture.OutputDirectory,
            CommandText = $"./a.out {initialEventToSelectTest} {testEvents}"
        };
        BashRunner.RunCommand(process);

        string output = process.StdOutput;
        output = StringUtils.RemoveEverythingBefore(output, "\nIGNORE_OUTPUT_BEFORE_THIS\n").Trim();
        output = Regex.Replace(output, @"[\s\S]*\nCLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\n[\s\S]*?\n\n", "").Trim();

        output = PreProcessOutput(output);

        return output;
    }

    public string PrepExpectedOutput(string expected)
    {
        return SpecTester.PrepExpectedOutput(expected);
    }

    public string PreProcessOutput(string output)
    {
        output = Regex.Replace(output, @"\w+__([a-zA-Z]+)", "$1");
        return output;
    }

}



