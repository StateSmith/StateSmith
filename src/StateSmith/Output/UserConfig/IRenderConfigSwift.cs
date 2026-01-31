namespace StateSmith.Output.UserConfig;

public interface IRenderConfigSwift : IRenderConfig
{
    string Imports => "";
    string BaseList => "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode => "";
}
