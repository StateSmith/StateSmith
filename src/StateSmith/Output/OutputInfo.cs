#nullable enable

namespace StateSmith.Output;

public class OutputInfo : IOutputInfo
{
    public string outputDirectory = "";

    public string OutputDirectory => outputDirectory;

    /// <summary>
    /// output file name without extension
    /// </summary>
    public string baseFileName = "";

    public string BaseFileName => baseFileName;
}
