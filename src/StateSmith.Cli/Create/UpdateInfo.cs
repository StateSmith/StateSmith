using System;

namespace StateSmith.Cli.Create;

public class UpdateInfo
{
    public const string DefaultStateSmithLibVersion = "0.9.9-alpha";

    public string LastestStateSmithLibStableVersion { get; set; } = DefaultStateSmithLibVersion;
    public string LastCheckDateTime { get; set; } = "2021-09-01T00:00:00Z";

    public double GetMsSinceLastCheck()
    {
        var lastCheck = DateTime.Parse(LastCheckDateTime);
        var now = DateTime.Now;
        return (now - lastCheck).TotalMilliseconds;
    }
}
