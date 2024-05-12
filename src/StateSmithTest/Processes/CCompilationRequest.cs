#nullable enable

using System.Collections.Generic;

namespace StateSmithTest.Processes;

public class CCompilationRequest
{
    public string WorkingDirectory = "";

    /// <summary>
    /// This can be relative to working directory or absolute paths.
    /// </summary>
    public required List<string> SourceFiles;

    /// <summary>
    /// This can be relative to working directory or absolute paths.
    /// </summary>
    public List<string> IncludePaths = [];
}
