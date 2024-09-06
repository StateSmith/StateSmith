#nullable enable

namespace StateSmith.Runner;

public enum TranspilerId
{
    /// <summary>
    /// This is a valid option if the user specifies the transpiler ID within the diagram file.
    /// </summary>
    NotYetSet = -1,
    Default,

    C99,
    CSharp,
    JavaScript,
    Java,
}


