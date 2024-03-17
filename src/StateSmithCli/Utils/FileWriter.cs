using System.IO;

namespace StateSmithCli.Utils;

public class FileWriter : IFileWriter
{
    public void Write(string path, string content)
    {
        File.WriteAllText(path: path, contents: content);
    }
}
