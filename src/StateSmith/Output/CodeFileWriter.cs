using System.IO;

namespace StateSmith.Output;

public class CodeFileWriter : ICodeFileWriter
{
    public void WriteFile(string filePath, string code)
    {
        File.WriteAllText(path:filePath, code);
    }
}

