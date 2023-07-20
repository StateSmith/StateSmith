#nullable enable

namespace StateSmith.Output;

public interface IOutputInfo
{
    string OutputDirectory { get; }

    /// <summary>
    /// output file name without extension
    /// </summary>
    string BaseFileName { get; }
}
