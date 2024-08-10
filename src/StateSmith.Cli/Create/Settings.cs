using StateSmith.Runner;

namespace StateSmith.Cli.Create;

public static class FileExtensionId
{
    public const string PlantUml = ".plantuml";
    public const string DrawIo = ".drawio";
    public const string DrawIoSvg = ".drawio.svg";
}

/// <summary>
/// properties are saved to settings.json, fields are not
/// </summary>
public class Settings
{
    //------------ PROPERTIES ARE PERSISTED ------------

    /// <summary>
    /// Persisted. Default is false (want diagram only to be default).
    /// </summary>
    public bool UseCsxWorkflow { get; set; } = false;

    /// <summary>
    /// Persisted
    /// </summary>
    public string StateSmithVersion { get; set; } = UpdateInfo.DefaultStateSmithLibVersion;

    /// <summary>
    /// Persisted
    /// TODO - This should maybe be changed to an enum.
    /// </summary>
    public string FileExtension { get; set; } = FileExtensionId.PlantUml;

    /// <summary>
    /// Persisted
    /// </summary>
    public TargetLanguageId TargetLanguageId { get; set; } = TargetLanguageId.C;

    /// <summary>
    /// Persisted
    /// </summary>
    public string DrawIoDiagramTemplateId { get; set; } = TemplateIds.DrawIoPages1; // string to accommodate user templates someday

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
    public string diagramFileName = "";

    /// <summary>
    /// not persisted
    /// </summary>
    public string scriptFileName = "";

    public bool IsDrawIoSelected()
    {
        return FileExtension == FileExtensionId.DrawIo || FileExtension == FileExtensionId.DrawIoSvg;
    }

    public bool IsDrawIoSvgSelected()
    {
        return FileExtension == FileExtensionId.DrawIoSvg;
    }

    public bool IsPlantUmlSelected()
    {
        return FileExtension == FileExtensionId.PlantUml;
    }

    public string GetTemplateId()
    {
        return IsDrawIoSelected() ? DrawIoDiagramTemplateId : PlantUmlDiagramTemplateId;
    }
}
