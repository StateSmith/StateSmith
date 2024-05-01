using StateSmith.Cli.Run;

namespace StateSmith.Cli;

public class Manifest
{
    public ManifestVersion Version { get; set; } = new(0, 1, 0);
    public RunManifest RunManifest { get; set; } = new();

    public class ManifestVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public string tag { get; set; } = "";

        public ManifestVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
    }
}
