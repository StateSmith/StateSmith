using System.Collections.Generic;
using System.Threading;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Versioning;
using System.Threading.Tasks;
using NuGet.Common;
using Spectre.Console;
using System.Linq;

namespace StateSmith.Cli.Utils;

public class NugetVersionGrabber
{
    public static async Task<IEnumerable<NuGetVersion>> GetVersions(string projectName)
    {
        IEnumerable<IPackageSearchMetadata> packages = await GetPackageMetadata(projectName);
        return packages.Select(p => p.Identity.Version);
    }

    public static async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadata(string projectName)
    {
        // see https://github.com/NuGet/Home/issues/12370
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        PackageMetadataResource resource = await repository.GetResourceAsync<PackageMetadataResource>();

        IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
            projectName,
            includePrerelease: true,
            includeUnlisted: false,
            cache,
            logger,
            cancellationToken);
        return packages;
    }
}
