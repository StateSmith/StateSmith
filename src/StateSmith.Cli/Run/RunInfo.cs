using StateSmith.Cli.Utils;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class RunInfo
{
    public string dirOrManifestPath;
    public Dictionary<string, CsxRunInfo> csxRuns = new();

    public RunInfo(string dirOrManifestPath)
    {
        this.dirOrManifestPath = dirOrManifestPath;
    }

    /// <summary>
    /// Not the fastest, but it works and is simple. We don't need to optimize this.
    /// </summary>
    /// <returns></returns>
    public RunInfo DeepCopy()
    {
        JsonFilePersistence jsonFilePersistence = new();
        jsonFilePersistence.IncludeFields = true;
        var json = jsonFilePersistence.PersistToString(this);
        return jsonFilePersistence.RestoreFromString<RunInfo>(json);
    }
}
