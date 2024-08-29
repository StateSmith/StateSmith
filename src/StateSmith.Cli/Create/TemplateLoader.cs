using System;
using System.Reflection;

namespace StateSmith.Cli.Create;

public class TemplateLoader
{
    public static readonly string basePath = "StateSmith.Cli.Create.Templates.";
    public const string DiagramFileNamePrefix = "diagram";
    public const string DrawIoFileName = DiagramFileNamePrefix + ".drawio";
    public const string PlantUmlFileName = DiagramFileNamePrefix + ".plantuml";
    public const string CsxFileName = "script.csx";

    public enum TomlConfigType
    {
        Most,
        Minimal
    }

    public static string LoadDefaultCsx()
    {
        var result = LoadFileResource(TemplateIds._common, fileName: "default.csx");
        return result;
    }

    public static string LoadTomlConfig(TomlConfigType tomlConfigType)
    {
        var result = LoadFileResource(TemplateIds._common, fileName: $"config-{tomlConfigType.ToString().ToLower()}.toml");
        return result;
    }

    public static string LoadCsxOrDefault(string templateName)
    {
        string? result = MaybeLoadFileResource(templateName, fileName: CsxFileName);
        result ??= LoadDefaultCsx();
        return result;
    }

    public static string LoadCsx(string templateName)
    {
        var result = LoadFileResource(templateName, fileName: CsxFileName);
        return result;
    }

    public static string LoadDrawio(string templateName)
    {
        var result = LoadFileResource(templateName, fileName: DrawIoFileName);
        return result;
    }

    public static string LoadPlantUml(string templateName)
    {
        var result = LoadFileResource(templateName, fileName: PlantUmlFileName);
        return result;
    }

    public static string LoadDiagram(string templateName, bool isDrawIoSelected)
    {
        return isDrawIoSelected ? LoadDrawio(templateName) : LoadPlantUml(templateName);
    }

    public static string LoadFileResource(string templateName, string fileName)
    {
        string? result = MaybeLoadFileResource(templateName, fileName);
        
        if (result == null)
        {
            var path = GetResourcePath(templateName, fileName);
            throw new InvalidOperationException($"Resource not found: {path}");
        }

        return result;
    }

    public static string? MaybeLoadFileResource(string templateName, string fileName)
    {
        // See https://stackoverflow.com/questions/3314140/how-to-read-embedded-resource-text-file
        string resourcePath = GetResourcePath(templateName, fileName);

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
            return null;

        using var reader = new System.IO.StreamReader(stream);
        string result = reader.ReadToEnd();

        return result;
    }

    private static string GetResourcePath(string templateName, string fileName)
    {
        string path = $"{basePath}{templateName}.";
        path = path.Replace("-", "_"); // embedded resource directory path has dashes to underscores. https://stackoverflow.com/questions/14705211/how-is-net-renaming-my-embedded-resources
        path += fileName; // dashes are fine in file names
        return path;
    }
}
