using System.Collections.Generic;
using System.Threading;
using System;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Versioning;
using System.Threading.Tasks;
using NuGet.Common;

namespace StateSmithCli.Create;

public class NugetVersionGrabber
{
    public static async Task RunAsync()
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
            Console.WriteLine($"Found version {version}");
        }
    }
}
