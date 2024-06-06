using StateSmith.Runner;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class StandardFiles
{
    /// <summary>
    /// Example: ".csx", ".drawio.svg", ...
    /// </summary>
    public static List<string> GetStandardFileExtensions()
    {
        List<string> list = DiagramFileAssociator.GetAllDiagramExtensions();
        list.Insert(index: 0, ".csx");
        return list;
    }
}
