using StateSmith.Cli.Utils;
using System.IO;

namespace StateSmith.Cli.Manifest;

public class ManifestPersistance : IManifestPersistance
{
    public const string ManifestFileName = "statesmith.cli.json";
    string dirPath;

    public ManifestPersistance(string dirPath)
    {
        this.dirPath = dirPath;
    }

    public ManifestData ReadOrThrow()
    {
        string filePath = GetManifestPath();
        JsonFilePersistence jsonFilePersistence = new() { IncludeFields = true };
        return jsonFilePersistence.RestoreFromFile<ManifestData>(filePath);
    }

    public ManifestData? ReadIfExistsAndValid()
    {
        string filePath = GetManifestPath();
        JsonFilePersistence jsonFilePersistence = new() { IncludeFields = true };
        try
        {
            return jsonFilePersistence.RestoreFromFile<ManifestData>(filePath);
        }
        catch
        {
            return null;
        }
    }

    public bool ManifestExists()
    {
        string filePath = GetManifestPath();
        return File.Exists(filePath);
    }

    // todolow - use interface instead of static method
    public static bool ManifestExists(string dirPath)
    {
        string filePath = Path.Combine(dirPath, ManifestFileName);
        return File.Exists(filePath);
    }

    public void Write(ManifestData manifest, bool overWrite = false)
    {
        string filePath = GetManifestPath();

        if (!overWrite && File.Exists(filePath))
            throw new("Manifest file already exists. Use overWrite = true to overwrite.");

        JsonFilePersistence jsonFilePersistence = new() { IncludeFields = true };
        jsonFilePersistence.PersistToFile(manifest, filePath);
    }

    public bool IsOverWriteRequired()
    {
        string filePath = GetManifestPath();
        return File.Exists(filePath);
    }

    public string GetManifestPath()
    {
        return Path.Combine(dirPath, ManifestFileName);
    }
}
