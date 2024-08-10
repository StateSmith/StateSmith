using StateSmith.Cli.Utils;
using StateSmith.Common;
using StateSmith.Output;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace StateSmith.Cli.Create;

public class Generator
{
    private Settings settings;
    private IFileWriter fileWriter = new FileWriter();
    internal TemplateLoader.TomlConfigType tomlConfigType = TemplateLoader.TomlConfigType.Minimal;

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
        if (settings.UseCsxWorkflow)
        {
            GenerateCsx();
        }
        GenerateDiagramFile();
    }

    public void GenerateCsx()
    {
        var templateStr = TemplateLoader.LoadCsxOrDefault(settings.GetTemplateId());

        string diagramFilePathRelative = GetDiagramPathRelativeToCsx();

        var r = new CsxTemplateRenderer(settings.TargetLanguageId, stateSmithVersion: settings.StateSmithVersion, diagramPath: diagramFilePathRelative, smName: settings.smName, template: templateStr);
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

    internal static string GetTomlConfig(TargetLanguageId targetLanguageId, TemplateLoader.TomlConfigType tomlConfigType)
    {
        var tomlConfigTemplate = TemplateLoader.LoadTomlConfig(tomlConfigType);
        var filterEngine = new TemplateFilterEngine();
        var result = filterEngine.ProcessAllFilters(tomlConfigTemplate, filterTag: targetLanguageId.ToString());
        
        return result;
    }

    public void GenerateDiagramFile()
    {
        string diagramTemplateStr = GenerateDiagramFileText();
        fileWriter.Write(settings.diagramFileName, diagramTemplateStr);
    }

    public string GenerateDiagramFileText()
    {
        var diagramTemplateStr = TemplateLoader.LoadDiagram(settings.GetTemplateId(), isDrawIoSelected: settings.IsDrawIoSelected());
        diagramTemplateStr = diagramTemplateStr.Replace("{{smName}}", settings.smName);
        diagramTemplateStr = SupportDefaultTomlConfig(diagramTemplateStr);

        var filterEngine = new TemplateFilterEngine();
        diagramTemplateStr = filterEngine.ProcessAllFilters(diagramTemplateStr, filterTag: settings.TargetLanguageId.ToString());

        if (settings.IsDrawIoSvgSelected())
        {
            diagramTemplateStr = WrapDrawioXmlForSvg(diagramTemplateStr);
        }

        return diagramTemplateStr;
    }

    private string SupportDefaultTomlConfig(string diagramTemplateStr)
    {
        var tomlConfig = GetTomlConfig(settings.TargetLanguageId, tomlConfigType);

        if (settings.UseCsxWorkflow == false)
        {
            // simpler workflow (no csx) so the toml should specify the transpilerId.
            // modify toml so transpilerId line is uncommented.
            tomlConfig = tomlConfig.Replace("# transpilerId = ", "transpilerId = ");
        }
        else
        {
            // csx workflow, so the toml should generally specify the transpilerId.
            tomlConfig = tomlConfig.Replace("transpilerId = ", "# transpilerId = ");
        }

        if (settings.IsDrawIoSelected())
        {
            // keep indentation. regular leading spaces are ignored by drawio.
            tomlConfig = new Regex(@"(?m)^([ ]+)").Replace(tomlConfig, x =>
            {
                const int nonBreakingSpace = 160;
                return x.Value.Replace(' ', (char)nonBreakingSpace);
            });
            tomlConfig = HttpUtility.HtmlEncode(tomlConfig);
            tomlConfig = StringUtils.ReplaceNewLineChars(tomlConfig, "&#10;");
        }

        diagramTemplateStr = diagramTemplateStr.Replace("{{configToml}}", tomlConfig);
        return diagramTemplateStr;
    }

    public static string WrapDrawioXmlForSvg(string drawioXml)
    {
        string wrapper = TemplateLoader.LoadFileResource("_global-svg", "svg-wrapper.drawio.svg");
        drawioXml = HttpUtility.HtmlEncode(drawioXml);

        // todo low - we would ideally use a proper xml library to do this. Regex is OK because we control the template file, but still...
        // NOTE! We can't use the drawioXml as-is for the regex replacement because it sometimes contains special text
        // that is interpreted as a replacement pattern like `$&` (include entire match). So we use a placeholder string instead.
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/substitutions-in-regular-expressions
        const string SafePlaceHolderToReplaceWithXml = "SafePlaceHolderToReplaceWithXml";
        wrapper = Regex.Replace(wrapper, "content\\s*=\\s*\"[^\"]+\"", $"content=\"{SafePlaceHolderToReplaceWithXml}\"");
        wrapper = wrapper.Replace(SafePlaceHolderToReplaceWithXml, drawioXml);
        return wrapper;
    }
}
