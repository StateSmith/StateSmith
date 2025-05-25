namespace StateSmith.Output.UserConfig;

public interface IRenderConfigJava : IRenderConfig
{
    string Package => "";
    string Imports => "";
    string Extends => "";
    string Implements => "";

    /// <summary>
    /// Disables generation of statemachine delegates
    /// </summary>
    string NoDelegate => ""; // TODO bool?

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode => "";
}
