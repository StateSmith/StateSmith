using System;

namespace StateSmith.Cli.Utils;

/// <summary>
/// dotnet-script program helper
/// </summary>
public class DotnetScriptProgram
{
    public const string Name = "dotnet-script";

    /// <summary>
    /// Either string (success) or exception will be non-null.
    /// </summary>
    public static (string?, Exception?) TryGetVersionString()
    {
        try
        {
            SimpleProcess process = new()
            {
                SpecificCommand = Name,
                SpecificArgs = "--version",
                throwOnExitCode = true
            };
            process.Run(timeoutMs: 30000);

            // parse version into major, minor, patch
            string versionStr = process.StdOutputBuf.ToString().Trim();
            return (versionStr, null);
        }
        catch (Exception e)
        {
            return (null, e);
        }
    }
}
