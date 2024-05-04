using StateSmith.Cli.Run;

namespace StateSmith.Cli;

/// <summary>
/// Stored in json file
/// </summary>
public class Manifest : Versionable
{
    public RunManifest RunManifest { get; set; } = new();
}
