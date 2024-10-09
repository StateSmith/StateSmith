using System;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

/// <summary>
/// NOTE! Data is persisted!
/// </summary>
public class DiagramRunInfo : RunInfo
{
    /// <summary>
    /// NOTE! Field is persisted.
    /// </summary>
    public string absolutePath;

    public DiagramRunInfo(string absolutePath)
    {
        this.absolutePath = absolutePath;
    }

    public override List<string> GetSourceFileAbsolutePaths()
    {
        var list = new List<string>();
        list.Add(absolutePath);
        return list;
    }
}
