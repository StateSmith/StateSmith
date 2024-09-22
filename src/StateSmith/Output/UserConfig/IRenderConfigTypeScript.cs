namespace StateSmith.Output.UserConfig;

public interface IRenderConfigTypeScript : IRenderConfig
{
    string Extends => "";

    string Implements => "";

    /// <summary>
    /// Use to add custom code to generated state machine class, although <seealso cref="Extends"/>
    /// may suit your needs better.
    /// </summary>
    string ClassCode => "";
}
