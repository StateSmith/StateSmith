#nullable enable
using StateSmith.SmGraph;
using System.Runtime.CompilerServices;

namespace StateSmith.Input.Expansions;

public class UserExpansionScriptBase
{
    /// <summary>
    /// This value will be updated as necessary for target language and rendering engine
    /// </summary>
    public string VarsPath { get; internal set; } = null!;

    /// <summary>
    /// The current vertex being processed.
    /// Experimental! API may change here. Maybe to make returned object immutable.
    /// </summary>
    public Vertex CurrentVertex => CurrentBehavior.OwningVertex;

    /// <summary>
    /// The current behavior being processed.
    /// Experimental! API may change here. Maybe to make returned object immutable.
    /// </summary>
    public Behavior CurrentBehavior { get; internal set; } = null!;

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
