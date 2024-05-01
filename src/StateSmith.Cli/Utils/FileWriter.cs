using System;
using System.IO;

namespace StateSmith.Cli.Utils;

public class FileWriter : IFileWriter
{
    public void Write(string path, string content)
    {
        File.WriteAllText(path: path, contents: content);
    }
}
