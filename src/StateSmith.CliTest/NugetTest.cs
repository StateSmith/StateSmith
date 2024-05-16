using System;
using Xunit;
using StateSmith.Cli.Utils;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using NuGet.Versioning;

namespace StateSmithCliTest;

public class NugetTest
{
    [Fact]
    public void Test1()
    {
        var versions = NugetVersionGrabber.GetVersions("StateSmith").Result.ToList();
        ShouldntFindUnlisted(versions);
        ShouldFindListed(versions);
    }

    private static void ShouldFindListed(List<NuGetVersion> versions)
    {
        List<string> expected = [
            "0.9.13-alpha",
            "0.9.12-alpha",
            "0.5.0-alpha",
            "0.7.10-alpha",
        ];

        foreach (var version in versions)
        {
            var v = version.ToFullString();
            expected.Remove(v);
        }

        expected.Should().BeEmpty("All listed versions should have been found");
    }

    private static void ShouldntFindUnlisted(List<NuGetVersion> versions)
    {
        string[] unlistedVersions = [
            "0.9.13-alpha-tracking-expander",
            "0.9.13-alpha-tracking-expander2",
            "0.9.8",
            "0.8.1-alpha-test",
            "0.0.1000-nugettest",
        ];

        foreach (var version in versions)
        {
            var v = version.ToFullString();
            if (unlistedVersions.Contains(v))
            {
                throw new Exception($"Unlisted version found: {v}");
            }
        }
    }
}
