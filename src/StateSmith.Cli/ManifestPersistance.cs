using StateSmith.Cli.Utils;
using System.IO;

namespace StateSmith.Cli;

public class ManifestPersistance
{
    string dirPath;

    public ManifestPersistance(string dirPath)
    {
        this.dirPath = dirPath;
    }

    public Manifest? Read()
    {
        string filePath = GetManifestPath();
        JsonFilePersistence jsonFilePersistence = new() { IncludeFields = true };
        try
        {
            return jsonFilePersistence.RestoreFromFile<Manifest>(filePath);
        }
        catch
        {
            return null;
        }
    }

    public void Write(Manifest manifest, bool overWrite = false)
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
        return Path.Combine(dirPath, "statesmith.manifest.json");
    }
}
