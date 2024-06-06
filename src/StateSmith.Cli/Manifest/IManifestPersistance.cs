namespace StateSmith.Cli.Manifest;

public interface IManifestPersistance
{
    ManifestData ReadOrThrow();
    ManifestData? ReadIfExistsAndValid();
    bool ManifestExists();
    void Write(ManifestData manifest, bool overWrite = false);
    bool IsOverWriteRequired();
    string GetManifestPath();
}
