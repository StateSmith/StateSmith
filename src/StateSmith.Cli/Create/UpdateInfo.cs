using StateSmith.Output;
using System;

namespace StateSmith.Cli.Create;

public class UpdateInfo
{
    public static readonly string DefaultStateSmithLibVersion = GetDefaultStateSmithLibVersion();
    public string LatestStateSmithLibStableVersion { get; set; } = DefaultStateSmithLibVersion;
    public string LastCheckDateTime { get; set; } = "2021-09-01T00:00:00Z";

    static string GetDefaultStateSmithLibVersion()
    {
        return LibVersionInfo.GetVersionInfoString(removeBuildInfo: true);
    }

    public double GetMsSinceLastCheck()
    {
        var lastCheck = DateTime.Parse(LastCheckDateTime);
        var now = DateTime.Now;
        return (now - lastCheck).TotalMilliseconds;
    }
}
