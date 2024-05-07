using StateSmith.Cli.Run;

namespace StateSmith.Cli.Manifest;

/// <summary>
/// Stored in json file
/// </summary>
public class ManifestData : Versionable
{
    public RunManifest RunManifest { get; set; } = new();
}
