namespace StateSmith.Output.UserConfig;

public interface IRenderConfigBerry : IRenderConfig
{
    /// <summary>
    /// Additional text inserted near the top of the generated Berry source file.
    /// </summary>
    string Imports => "";

    /// <summary>
    /// Optional base class (provide "MyBase" to emit `class MySm : MyBase`).
    /// </summary>
    string Extends => "";

    /// <summary>
    /// Arbitrary Berry code appended inside the generated state machine class after the opening line.
    /// </summary>
    string ClassCode => "";
}
