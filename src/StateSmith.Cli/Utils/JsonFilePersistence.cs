using System.IO;
using System.Text.Json;

namespace StateSmith.Cli.Utils;

public class JsonFilePersistence
{
    public static void PersistToFile<T>(T obj, string filePath)
    {
        string json = PersistToString(obj);
        File.WriteAllText(filePath, json);
    }

    public static string PersistToString<T>(T obj)
    {
        JsonSerializerOptions serializeOptions = GetSerializeOptions();
        string json = JsonSerializer.Serialize(obj, serializeOptions);
        return json;
    }

    public static T RestoreFromFile<T>(string filePath)
    {
        string json = File.ReadAllText(filePath);
        T obj = RestoreFromString<T>(json);
        return obj;
    }

    public static T RestoreFromString<T>(string json)
    {
        JsonSerializerOptions serializeOptions = GetSerializeOptions();
        var obj = JsonSerializer.Deserialize<T>(json, serializeOptions);

        if (obj == null)
            throw new System.Exception($"Failed to deserialize");
        return obj;
    }

    private static JsonSerializerOptions GetSerializeOptions()
    {
        var serializeOptions = new JsonSerializerOptions
        {
            IncludeFields = false, // we don't want to persist fields
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            //ReadCommentHandling = JsonCommentHandling.Skip;
            WriteIndented = true,
        };
        return serializeOptions;
    }
}
