using System.IO;
using System;

namespace StateSmith.Output;

/// <summary>
/// Allows us to capture the code that would be written to a file.
/// </summary>
public class SingleFileCapturer : ICodeFileWriter
{
    public string CapturedCode { get; private set; } = "";
    public string FilePath { get; private set; } = "";

    void ICodeFileWriter.WriteFile(string filePath, string code)
    {
        if (CapturedCode != "")
        {
            throw new InvalidOperationException(nameof(SingleFileCapturer) + " can only capture one file. Already captured file: " + FilePath);
        }

        FilePath = filePath;
        CapturedCode = code;
    }

    public string FileName => Path.GetFileName(FilePath);
}
