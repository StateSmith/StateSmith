using System.Text.Json.Serialization;

namespace StateSmith.Cli.Create;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
    //------------ PROPERTIES ARE PERSISTED ------------

    /// <summary>
    /// Persisted
    /// </summary>
    public string StateSmithVersion { get; set; } = UpdateInfo.DefaultStateSmithLibVersion;

    /// <summary>
    /// Persisted
    /// This should maybe be changed to an enum.
    /// </summary>
    public string FileExtension { get; set; } = ".drawio";

    /// <summary>
    /// Persisted
    /// </summary>
    public TargetLanguageId TargetLanguageId { get; set; } = TargetLanguageId.C;

    /// <summary>
    /// Persisted
    /// </summary>
    public string DrawIoDiagramTemplateId { get; set; } = TemplateIds.DrawIoSimple1; // string to accommodate user templates someday

    /// <summary>
    /// Persisted
    /// </summary>
    public string PlantUmlDiagramTemplateId { get; set; } = TemplateIds.PlantUmlSimple1;    // string to accommodate user templates someday


    //------------ FILEDS ARE NOT PERSISTED ------------

    /// <summary>
    /// not persisted
    /// </summary>
    public string smName = "MySm";

    /// <summary>
    /// not persisted
    /// </summary>
    public string diagramFileName = ".drawio";

    /// <summary>
    /// not persisted
    /// </summary>
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
