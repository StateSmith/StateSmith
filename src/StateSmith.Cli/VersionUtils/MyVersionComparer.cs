using NuGet.Versioning;
using System;

namespace StateSmith.Cli.VersionUtils;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/351
/// </summary>
public class MyVersionComparer
{
    /// <summary>
    /// Return false if either version is null
    /// </summary>
    public static bool IsOtherVersionGreater(SemanticVersion? currentVersion, SemanticVersion? otherVersion)
    {
        if (currentVersion == null || otherVersion == null)
        {
            return false;
        }

        return otherVersion > currentVersion;
    }
}
