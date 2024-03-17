using System;
using System.Reflection;

namespace StateSmithCli.Create;

public class TemplateLoader
{
    public static readonly string basePath = "StateSmithCli.Create.Templates.";

    public static string LoadCsx(string templateName)
    {
        var result = LoadResource(templateName, fileExtension: ".csx");
        return result;
    }

    public static string LoadResource(string templateName, string fileExtension)
    {
        // See https://stackoverflow.com/questions/3314140/how-to-read-embedded-resource-text-file
        var assembly = Assembly.GetExecutingAssembly();
        string path = basePath + templateName + ".MySmName" + fileExtension;
        path = path.Replace("-", "_"); // embedded resource name has dashes to underscores. https://stackoverflow.com/questions/14705211/how-is-net-renaming-my-embedded-resources
        using var stream = assembly.GetManifestResourceStream(path);

        if (stream == null)
            throw new Exception($"Could not find resource {path}");

        using var reader = new System.IO.StreamReader(stream);
        string result = reader.ReadToEnd();

        return result;
    }
}
