namespace StateSmith.Output.UserConfig;

public interface IRenderConfigSwift : IRenderConfig
{
    string Imports => "";
    string Extends => "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode => "";
}
