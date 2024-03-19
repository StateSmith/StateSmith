using StateSmith.Cli.Utils;
using System.Text.RegularExpressions;
using System.Web;

namespace StateSmith.Cli.Create;

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
        var templateStr = TemplateLoader.LoadDiagram(templateName, isDrawIoSelected: settings.IsDrawIoSelected());
        var result = templateStr.Replace("{{smName}}", settings.smName);

        if (settings.IsDrawIoSvgSelected())
        {
            result = WrapDrawioXmlForSvg(result);
        }

        fileWriter.Write(settings.diagramFileName, result);
    }

    public static string WrapDrawioXmlForSvg(string drawioXml)
    {
        string wrapper = TemplateLoader.LoadFileResource("_global-svg", "svg-wrapper.drawio.svg");
        drawioXml = HttpUtility.HtmlEncode(drawioXml);
        wrapper = Regex.Replace(wrapper, "content\\s*=\\s*\"[^\"]+\"", $"content=\"{drawioXml}\"");
        return wrapper;
    }
}
