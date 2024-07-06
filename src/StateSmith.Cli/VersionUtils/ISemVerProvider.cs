using NuGet.Versioning;

namespace StateSmith.Cli.VersionUtils;

public interface ISemVerProvider
{
    SemanticVersion GetVersion();
}
