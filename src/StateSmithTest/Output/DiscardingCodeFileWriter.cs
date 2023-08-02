#nullable enable

using StateSmith.Output;

namespace StateSmithTest.Output;

public class DiscardingCodeFileWriter : ICodeFileWriter
{
    public void WriteFile(string filePath, string code)
    {
        // ignore it
    }
}
