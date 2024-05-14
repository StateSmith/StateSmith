#nullable enable

using System.Collections.Generic;

namespace StateSmithTest.Processes.CComp;

public class CCompRequest
{
    public enum FlagId
    {
        /// <summary>
        /// GCC: -Wno-unused-function
        /// </summary>
        IgnoreUnusedFunctions,
    }

    public string WorkingDirectory = "";

    /// <summary>
    /// If not null, this is the path to the compiler executable.
    /// </summary>
    public string? CompilerPath = null;

    /// <summary>
    /// This can be relative to working directory or absolute paths.
    /// </summary>
    public required List<string> SourceFiles;

    /// <summary>
    /// This can be relative to working directory or absolute paths.
    /// </summary>
    public List<string> IncludePaths = [];

    public List<FlagId> Flags = [];
}
