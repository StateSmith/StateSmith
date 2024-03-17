namespace StateSmithCli.Create;

public enum TargetLanguageId
{
    C,
    CppC,
    CSharp,
    JavaScript,
}

/// <summary>
/// properties are saved to settings.json, fields are not
/// </summary>
public class Settings
{
    public string StateSmithVersion { get; set; } = "0.9.8-alpha";

    /// <summary>
    /// This should maybe be changed to an enum
    /// </summary>
    public string FileExtension { get; set; } = ".drawio.svg";
    public TargetLanguageId TargetLanguageId { get; set; } = TargetLanguageId.CSharp;
    public string DrawIoDiagramTemplateId { get; set; } = "drawio-simple-1"; // string to accommodate user templates someday
    public string PlantUmlDiagramTemplateId { get; set; } = "advanced-1";    // string to accommodate user templates someday

    public string smName = "MySm";
    public string diagramFileName = ".drawio.svg";
    public string scriptFileName = "MySm.csx";

    public bool IsDrawIoSelected()
    {
        return FileExtension.Contains(".drawio");
    }

    public bool IsDrawIoSvgSelected()
    {
        return FileExtension.Contains(".drawio.svg");
    }
}
