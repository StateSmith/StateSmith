using Xunit;
using StateSmithTest.spec2;
using StateSmithTest.Processes;
using StateSmith.Output;
using System.Text.RegularExpressions;

namespace Spec.Spec2.CSharp;

public class Spec2TestsCSharp : Spec2Tests, IClassFixture<SharedCompilationFixture>
{
    public override string RunProcess(string testEvents)
    {
        SimpleProcess process = new()
        {
            WorkingDirectory = SharedCompilationFixture.OutputDirectory,
            ProgramPath = "dotnet",
            //Args = $"run --no-build {testEvents}"   // slow compared to below
            Args = $"bin/Debug/net8.0/Spec2Test.dll {testEvents}" // 5x faster!!! Tip from https://github.com/dotnet/sdk/issues/8697#issuecomment-327943066
        };
        process.Run(SimpleProcess.DefaultLongTimeoutMs);

        string output = process.StdOutput;
        output = StringUtils.RemoveEverythingBefore(output, "\nIGNORE_OUTPUT_BEFORE_THIS\n").Trim();
        output = Regex.Replace(output, @"[\s\S]*\nCLEAR_OUTPUT_BEFORE_THIS_AND_FOR_THIS_EVENT_DISPATCH\n[\s\S]*?\n\n", "").Trim();

        output = PreProcessOutput(output);

        return output;
    }
}

