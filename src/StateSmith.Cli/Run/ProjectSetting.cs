namespace StateSmith.Cli.Run;

// TODOLOW remove this class if still not used
public class ProjectSetting
{
    public string CsxPath { get; set; } = "";

    public ProjectSetting(string csxPath)
    {
        this.CsxPath = csxPath;
    }
}
