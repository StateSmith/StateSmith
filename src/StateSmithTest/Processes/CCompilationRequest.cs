#nullable enable

using System.Collections.Generic;

namespace StateSmithTest.Processes;

public class CCompilationRequest
{
    public enum FlagId
    {
        /// <summary>
        /// GCC: -Wno-unused-function
        /// </summary>
        IgnoreUnusedFunctions,

        // -Wignored-qualifiers for https://github.com/StateSmith/StateSmith/issues/150
    }

    public string WorkingDirectory = "";

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
