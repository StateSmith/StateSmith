using System;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

/// <summary>
/// NOTE! Data is persisted!
/// </summary>
public class CsxRunInfo : RunInfo
{
    /// <summary>
    /// NOTE! Field is persisted.
    /// </summary>
    public string csxAbsolutePath;

    /// <summary>
    /// NOTE! Field is persisted.
    /// </summary>
    public List<string> diagramAbsolutePaths = new();

    public CsxRunInfo(string csxAbsolutePath)
    {
        this.csxAbsolutePath = csxAbsolutePath;
    }

    public override List<string> GetSourceFileAbsolutePaths()
    {
        var list = new List<string>();
        list.Add(csxAbsolutePath);
        list.AddRange(diagramAbsolutePaths);
        return list;
    }
}
