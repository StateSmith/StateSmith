using System.IO;
using System.Text.Json;

namespace StateSmith.Cli.Utils;

public class JsonFilePersistence
{
    public bool IncludeFields { get; set; } = true;

    public void PersistToFile<T>(T obj, string filePath)
    {
        string json = PersistToString(obj);
        File.WriteAllText(filePath, json);
    }

    public string PersistToString<T>(T obj)
    {
        JsonSerializerOptions serializeOptions = GetSerializeOptions();
        string json = JsonSerializer.Serialize(obj, serializeOptions);
        return json;
    }

    public T RestoreFromFile<T>(string filePath)
    {
        string json = File.ReadAllText(filePath);
        T obj = RestoreFromString<T>(json);
        return obj;
    }

    public T RestoreFromString<T>(string json)
    {
        JsonSerializerOptions serializeOptions = GetSerializeOptions();
        var obj = JsonSerializer.Deserialize<T>(json, serializeOptions);

        if (obj == null)
            throw new System.Exception($"Failed to deserialize");
        return obj;
    }

    private JsonSerializerOptions GetSerializeOptions()
    {
        var serializeOptions = new JsonSerializerOptions
        {
            IncludeFields = IncludeFields,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            //ReadCommentHandling = JsonCommentHandling.Skip;
            WriteIndented = true,
        };
        return serializeOptions;
    }
}
