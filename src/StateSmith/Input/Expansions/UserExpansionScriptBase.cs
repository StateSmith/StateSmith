#nullable enable
using System.Runtime.CompilerServices;

namespace StateSmith.Input.Expansions;

public class UserExpansionScriptBase
{
    /// <summary>
    /// This value will be updated as necessary for target language and rendering engine
    /// </summary>
    internal string? varsPath = null;

    /// <summary>
    /// This value will be updated as necessary for target language and rendering engine
    /// </summary>
    public string VarsPath => varsPath!;

    /// <summary>
    /// Expands to just the expansion name.
    /// </summary>
    public static string AutoNameCopy([CallerMemberName] string methodName = "")
    {
        return methodName;
    }

    /// <summary>
    /// Equivalent to `VarsPath + AutoNameCopy()`
    /// </summary>
    public string AutoVarName([CallerMemberName] string methodName = "")
    {
        return VarsPath + methodName;
    }
}
