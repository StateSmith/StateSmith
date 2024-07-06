#nullable enable
using System.Reflection;

namespace StateSmith.Output;

public class LibVersionInfo
{
    /// <summary>
    /// Returns semantic version info for StateSmith library. Something like "0.9.10-alpha+12345".
    /// The build information isn't always present.
    /// </summary>
    /// <param name="removeBuildInfo"></param>
    /// <returns></returns>
    public static string GetVersionInfoString(bool removeBuildInfo = false)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return GetVersionInfoString(assembly, removeBuildInfo);
    }

    /// <summary>
    /// Returns semantic version info for assembly. Something like "0.9.10-alpha+12345".
    /// The build information isn't always present.
    /// </summary>
    /// <returns></returns>
    public static string GetVersionInfoString(Assembly assembly, bool removeBuildInfo = false)
    {
        AssemblyInformationalVersionAttribute? attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        string versionInfo;

        if (attr != null)
        {
            versionInfo = attr.InformationalVersion;
            if (removeBuildInfo)
            {
                versionInfo = versionInfo.Split('+')[0];
            }
        }
        else
        {
            System.Version? version = assembly.GetName().Version;
            versionInfo = version?.ToString() + "-<unable-to-get-suffix>";
        }

        return versionInfo;
    }
}
