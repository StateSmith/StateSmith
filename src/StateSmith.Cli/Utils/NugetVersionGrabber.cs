using System.Collections.Generic;
using System.Threading;
using System;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Versioning;
using System.Threading.Tasks;
using NuGet.Common;
using Spectre.Console;

namespace StateSmith.Cli.Utils;

public class NugetVersionGrabber
{
    public static async Task RunAsync(IAnsiConsole console)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            "StateSmith",
            cache,
            logger,
            cancellationToken);

        foreach (NuGetVersion version in versions)
        {
            console.WriteLine($"Found version {version}");
        }
    }

    public static async Task<IEnumerable<NuGetVersion>> GetVersions(string projectName)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            projectName,
            cache,
            logger,
            cancellationToken);

        return versions;
    }
}
