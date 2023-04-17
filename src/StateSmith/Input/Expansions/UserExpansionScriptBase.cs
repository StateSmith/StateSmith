#nullable enable
using StateSmith.Common;
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
    /// The current named vertex being processed by code generation. Not runtime current.
    /// Experimental! API may change here. Maybe to make returned object immutable.
    /// https://github.com/StateSmith/StateSmith/issues/45
    /// 
    /// If a pseudo state with multiple incoming paths is involved, may return pseudo state's parent.
    /// </summary>
    public NamedVertex CurrentNamedVertex { get; internal set; } = null!;

    /// <summary>
    /// The current behavior being processed by code generation. Not runtime current.
    /// Experimental! API may change here. Maybe to make returned object immutable.
    /// https://github.com/StateSmith/StateSmith/issues/45
    /// </summary>
    public Behavior CurrentBehavior { get; internal set; } = null!;

    /// <summary>
    /// The current trigger name being processed by code generation. Not runtime current.
    /// Experimental! API may change here. The trigger name may already have been
    /// sanitized via <see cref="TriggerHelper.SanitizeTriggerName"/>.
    /// https://github.com/StateSmith/StateSmith/issues/45
    /// 
    /// May be blank if a pseudo state with multiple incoming paths is involved.
    /// </summary>
    public string CurrentTrigger { get; internal set; } = null! ?? "";

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
