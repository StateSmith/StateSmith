#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// The order of these values may change. Do not rely on the numeric value.
/// </summary>
public enum TranspilerId
{
    /// <summary>
    /// This is a valid option if the user specifies the transpiler ID within the diagram file.
    /// </summary>
    NotYetSet = -1,
    Default,

    C99,
    Cpp,
    CSharp,
    JavaScript,
    Java,
    Python,
    TypeScript,
    Swift,
}
