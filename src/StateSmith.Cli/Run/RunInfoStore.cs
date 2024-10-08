using StateSmith.Cli.Utils;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

/// <summary>
/// This class is persisted to disk in json format.
/// </summary>
public class RunInfoStore : Versionable
{
    // not sure we need this anymore
    public string dirOrManifestPath;

    /// <summary>
    /// Key is csx absolute path
    /// </summary>
    public Dictionary<string, CsxRunInfo> csxRuns = new();

    /// <summary>
    /// Key is diagram absolute path
    /// </summary>
    public Dictionary<string, DiagramRunInfo> diagramRuns = new();

    public RunInfoStore(string dirOrManifestPath)
    {
        this.dirOrManifestPath = dirOrManifestPath;
    }

    public string? FindCsxWithDiagram(string diagramAbsolutePath)
    {
        foreach (var csxRun in csxRuns.Values)
        {
            if (csxRun.diagramAbsolutePaths.Contains(diagramAbsolutePath))
            {
                return csxRun.csxAbsolutePath;
            }
        }

        return null;
    }

    /// <summary>
    /// Not the fastest, but it works and is simple. We don't need to optimize this.
    /// </summary>
    /// <returns></returns>
    public RunInfoStore DeepCopy()
    {
        JsonFilePersistence jsonFilePersistence = new();
        jsonFilePersistence.IncludeFields = true;
        var json = jsonFilePersistence.PersistToString(this);
        return jsonFilePersistence.RestoreFromString<RunInfoStore>(json);
    }
}
