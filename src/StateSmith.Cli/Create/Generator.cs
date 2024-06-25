using StateSmith.Cli.Utils;
using StateSmith.Common;
using System.IO;
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

        if (settings.UseCsxWorkflow)
        {
            GenerateCsx(templateName, settings.smName);
        }
        GenerateDiagramFile(templateName);
    }

    public void GenerateCsx(string templateName, string smName)
    {
        var templateStr = TemplateLoader.LoadCsxOrDefault(templateName);

        string diagramFilePathRelative = GetDiagramPathRelativeToCsx();

        var r = new TemplateRenderer(settings.TargetLanguageId, stateSmithVersion: settings.StateSmithVersion, diagramPath: diagramFilePathRelative, smName: smName, template: templateStr);
        var result = r.Render();
        fileWriter.Write(settings.scriptFileName, result);
    }

    private string GetDiagramPathRelativeToCsx()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var scriptAbsolutePath = PathUtils.EnsurePathAbsolute(settings.scriptFileName, currentDir);
        var diagramAbsolutePath = PathUtils.EnsurePathAbsolute(settings.diagramFileName, currentDir);
        string diagramFilePathRelative = Path.GetRelativePath(Path.GetDirectoryName(scriptAbsolutePath).ThrowIfNull(), diagramAbsolutePath);
        return diagramFilePathRelative;
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
