#nullable enable

using StateSmith.Output;

namespace StateSmithTest.Output;

public class CapturingCodeFileWriter : ICodeFileWriter
{
    public string code = "";
    public void WriteFile(string filePath, string code)
    {
        this.code = code;
    }
}
