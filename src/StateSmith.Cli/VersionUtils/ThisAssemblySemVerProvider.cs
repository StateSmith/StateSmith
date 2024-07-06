using CommandLine.Text;
using NuGet.Versioning;

namespace StateSmith.Cli.VersionUtils;

public class ThisAssemblySemVerProvider : ISemVerProvider
{
    public SemanticVersion GetVersion()
    {
        return SemanticVersion.Parse(Program.GetSemVersionString());
    }
}
