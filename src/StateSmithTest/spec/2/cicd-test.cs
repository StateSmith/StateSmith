using StateSmithTest.Processes;
using Xunit;

namespace StateSmithTest.spec2;

// This file is temporary.
// Will remove once we have swift MR merged

public class TempNewLangTests
{
    [Fact]
    public void CheckSwiftAvailable()
    {
        SimpleProcess process;

        process = new()
        {
            // WorkingDirectory = OutputDirectory,
            ProgramPath = "swift",
            Args = " --version"
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    [Fact]
    public void CheckKotlinAvailable()
    {
        SimpleProcess process;

        process = new()
        {
            ProgramPath = "kotlinc",
            Args = " -version",
            throwOnStdErr = false   // `kotlinc -version` prints to stderr so this is normal
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }
}
