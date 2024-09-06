namespace StateSmith.Output.UserConfig;

public interface IRenderConfigJava : IRenderConfig
{
    string Package => "";
    string Imports => "";
    string Extends => "";
    string Implements => "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode => "";

}
