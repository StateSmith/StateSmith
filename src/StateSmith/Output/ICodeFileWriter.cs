using System.Text;

namespace StateSmith.Output;

public interface ICodeFileWriter
{
    void WriteFile(string filePath, string code);
    void WriteFile(string filePath, StringBuilder code) => WriteFile(filePath, code.ToString());
}
