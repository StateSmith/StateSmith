using StateSmith.Common;
using System.IO;

#nullable enable

namespace StateSmith.Runner;

public class DiagramFilePathProvider
{
    string? filePath;

    public void SetDiagramFilePath(string filePath)
    {
        this.filePath = filePath.ThrowIfNull();
    }

    public string GetDiagramFilePath()
    {
        return filePath.ThrowIfNull();
    }

    public string GetDiagramFileName()
    {
        return Path.GetFileName(GetDiagramFilePath());
    }
}
