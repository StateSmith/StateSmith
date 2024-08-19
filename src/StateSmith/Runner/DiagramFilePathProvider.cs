#nullable enable

using StateSmith.Common;
using System.IO;

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

    public string GetDiagramBaseName()
    {
        // we don't just use Path.GetFileNameWithoutExtension because
        // files like MySm.drawio.svg would be returned as MySm.drawio
        return GetDiagramFileName().Split(".")[0];
    }
}
