using System;
using System.Collections.Generic;

namespace StateSmith.Cli.Run;

/// <summary>
/// NOTE! Data is persisted!
/// 
/// We store our own timestamps instead of just comparing source and output file write timestamps.
/// If git committed a changed csx file, but code gen wasn't run, then the generated files will be "dirty", but still have the same modified timestamp.
/// This would make it look like the generated files are up to date, when they are not.
/// </summary>
public abstract class RunInfo: Versionable
{
    /// <summary>
    /// NOTE! Field is persisted!
    /// Use for comparing against source files.
    /// </summary>
    public DateTime lastCodeGenStartDateTime = DateTime.Now;

    /// <summary>
    /// NOTE! Field is persisted!
    /// Use for comparing against generated files.
    /// End date time is important for checking output files as their last write time will ALWAYS
    /// be after the source file's last write time.
    /// </summary>
    public DateTime lastCodeGenEndDateTime = DateTime.Now;

    /// <summary>
    /// NOTE! Field is persisted!
    /// </summary>
    public List<string> writtenFileAbsolutePaths = new();

    /// <summary>
    /// NOTE! Field is persisted!
    /// </summary>
    public bool success;

    public abstract List<string> GetSourceFileAbsolutePaths();
}
