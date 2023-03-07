namespace StateSmith.Output.UserConfig;

public interface IRenderConfigCSharp : IRenderConfig
{
    string NameSpace => "";
    string Usings => "";

    /// <summary>
    /// Use to add custom code to generated state machine class
    /// </summary>
    string ClassCode => "";

    bool UseNullable => true;
}
