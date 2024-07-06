namespace StateSmith.Cli;

/// <summary>
/// Used to store semantic versioning info in json.
/// todolow - Should see if we could use NuGet.Versioning.SemanticVersion instead.
/// </summary>
public class StorableSemVersion
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string Tag { get; set; } = "";

    public StorableSemVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }
}
