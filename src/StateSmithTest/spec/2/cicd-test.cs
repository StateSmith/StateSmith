using StateSmithTest.Processes;
using Xunit;
using System.Runtime.InteropServices;
using System;

namespace StateSmithTest.spec2;

// This file is temporary.
// Will remove once we have swift MR merged

public class TempNewLangTests
{
    public static bool IsSwiftTestable()
    {
        bool isRunningOnMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        if (isRunningOnMac)
        {
            return true;
        }

        // don't run swift tests on github ubuntu runner. Just rely on mac runner for swift tests.
        return IsRunningOnGitHubActions() == false;
    }

    public static bool IsRunningOnGitHubActions()
    {
        string gitHubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        return !string.IsNullOrEmpty(gitHubActions) && gitHubActions.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckSwiftAvailable()
    {
        if (!IsSwiftTestable())
        {
            Console.WriteLine("NOTE!!! Skipping swift tests.");
            return;
        }

        SimpleProcess process = new()
        {
            ProgramPath = "swiftccc",
            Args = " --version",
            throwOnStdErr = false   // required for github runner
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }

    [Fact]
    public void CheckKotlinAvailable()
    {
        SimpleProcess process = new()
        {
            ProgramPath = "kotlinc",
            Args = " -version",
            throwOnStdErr = false   // `kotlinc -version` prints to stderr so this is normal
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }
}
