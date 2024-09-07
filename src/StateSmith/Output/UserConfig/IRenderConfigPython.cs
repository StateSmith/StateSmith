namespace StateSmith.Output.UserConfig;

public interface IRenderConfigPython : IRenderConfig
{
    string Imports => "";
    string Extends => "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode => "";
}
