using NuGet.Versioning;

namespace StateSmith.Cli.VersionUtils;

public class SimpleSemVerProvider : ISemVerProvider
{
    SemanticVersion semanticVersion;

    public SimpleSemVerProvider(SemanticVersion semanticVersion)
    {
        this.semanticVersion = semanticVersion;
    }

    public SimpleSemVerProvider(string version)
    {
        semanticVersion = SemanticVersion.Parse(version);
    }

    public void Set(string version)
    {
        semanticVersion = SemanticVersion.Parse(version);
    }

    public SemanticVersion GetVersion()
    {
        return semanticVersion;
    }
}
