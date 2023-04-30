#nullable enable

using StateSmith.Common;
using StateSmith.Output;

namespace StateSmithTest.Output;

/// <summary>
/// Captures what would normally be written to file to memory instead.
/// </summary>
public class CapturingCodeFileWriter : ICodeFileWriter
{
    /// <summary>
    /// indexed by filepath (key)
    /// </summary>
    public HashList<string, Capture> captures = new();
    public Capture? lastCapture = null;
    public string LastCode => lastCapture?.code ?? "";
    public int captureCount = 0;

    public void WriteFile(string filePath, string code)
    {
        Capture capture = new(filePath, code);
        lastCapture = capture;
        captures.Add(filePath, capture);
        captureCount++;
    }

    public class Capture
    {
        public string fileName;
        public string code;

        public Capture(string fileName, string code)
        {
            this.fileName = fileName;
            this.code = code;
        }
    }
}
