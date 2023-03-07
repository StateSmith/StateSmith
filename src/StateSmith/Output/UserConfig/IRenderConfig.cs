namespace StateSmith.Output.UserConfig;

public interface IRenderConfig
{
    string VariableDeclarations => "";

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/91
    /// </summary>
    string AutoExpandedVars => "";

    /// <summary>
    /// Not used yet. A comma seperated list of allowed event names. TODO case sensitive?
    /// </summary>
    string EventCommaList => "";

    string FileTop => "";
}

public class DummyIRenderConfig : IRenderConfig
{

}
