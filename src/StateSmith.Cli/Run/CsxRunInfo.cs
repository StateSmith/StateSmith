using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class CsxRunInfo
{
    public string csxAbsolutePath;   
    public List<string> diagramAbsolutePaths = new();

    public CsxRunInfo(string csxPath)
    {
        this.csxAbsolutePath = csxPath;
    }
}
