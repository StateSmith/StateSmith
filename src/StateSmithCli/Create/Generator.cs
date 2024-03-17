using StateSmithCli.Utils;
using System;
using System.IO;

namespace StateSmithCli.Create;

public class Generator
{
    private Settings settings;
    private IFileWriter fileWriter = new FileWriter();

    public Generator(Settings settings)
    {
        this.settings = settings;
    }

    public void SetFileWriter(IFileWriter fileWriter)
    {
        this.fileWriter = fileWriter;
    }

    public void GenerateFiles()
    {
        var templateName = settings.IsDrawIoSelected() ? settings.DrawIoDiagramTemplateId : settings.PlantUmlDiagramTemplateId;
        GenerateCsx(templateName);
        GenerateDiagramFile(templateName);
    }

    public void GenerateCsx(string templateName)
    {
        var templateStr = TemplateLoader.LoadCsx(templateName);
        var r = new CsxTemplateRenderer(settings.TargetLanguageId, stateSmithVersion: settings.StateSmithVersion, diagramPath: settings.diagramFileName, template: templateStr);
        var result = r.Render();
        fileWriter.Write(settings.scriptFileName, result);
    }

    public void GenerateDiagramFile(string templateName)
    {
        var inputFileExtension = settings.IsDrawIoSelected() ? ".drawio" : ".plantuml";
        var templateStr = TemplateLoader.LoadResource(templateName, fileExtension: inputFileExtension);
        var result = templateStr.Replace("{{smName}}", settings.smName);

        if (settings.IsDrawIoSvgSelected())
        {
            // TODO create global template SVG file that is an image saying to load file with drawio and save it again
            // TODO base64 encode.
            // TODO update template
            throw new Exception("SVG support not implemented yet");
        }

        fileWriter.Write(settings.diagramFileName, result);
    }
}
