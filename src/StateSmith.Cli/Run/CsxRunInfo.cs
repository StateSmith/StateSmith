using System;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

/// <summary>
/// Data is not persisted in the project directory.
/// We don't use the modified timestamp of the generated files because the csx file may change file extensions/paths.
/// Using git, they might not be accurate either.
/// If git committed a changed csx file, but code gen wasn't run, then the generated files will be "dirty", but still have the same modified timestamp.
/// This would make it look like the generated files are up to date, when they are not.
/// </summary>
public class CsxRunInfo : Versionable
{
    public string csxAbsolutePath;
    public List<string> diagramAbsolutePaths = new();
    public List<string> writtenFileAbsolutePaths = new();

    /// <summary>
    /// Use for comparing against source files.
    /// </summary>
    public DateTime lastCodeGenStartDateTime = DateTime.Now;

    /// <summary>
    /// Use for comparing against generated files.
    /// End date time is important for checking output files as their last write time will ALWAYS
    /// be after the source file's last write time.
    /// </summary>
    public DateTime lastCodeGenEndDateTime = DateTime.Now;
    public bool success;

    public CsxRunInfo(string csxAbsolutePath)
    {
        this.csxAbsolutePath = csxAbsolutePath;
    }
}
