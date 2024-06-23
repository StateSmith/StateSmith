namespace StateSmith.Output.UserConfig;

public interface IRenderConfig
{
    /// <summary>
    /// This section allows you to define custom variables for your state machine. Any code/text you put in here
    /// will be output directly inside the state machine variables object/struct.
    /// </summary>
    string VariableDeclarations => "";

    /// <summary>
    /// This section allows you to conveniently do two things at once: 1) define variables, 2) automatically create expansions for those variables.
    /// </summary>
    string AutoExpandedVars => "";

    /// <summary>
    /// Default variable expansion template.
    /// https://github.com/StateSmith/StateSmith/issues/284
    /// </summary>
    string DefaultVarExpTemplate => "";

    /// <summary>
    /// Default function expansion template.
    /// https://github.com/StateSmith/StateSmith/issues/284
    /// </summary>
    string DefaultFuncExpTemplate => "";

    /// <summary>
    /// Default variable and function expansion template.
    /// https://github.com/StateSmith/StateSmith/issues/284
    /// </summary>
    string DefaultAnyExpTemplate => "";

    /// <summary>
    /// Not used yet. A comma separated list of allowed event names. TODO case sensitive?
    /// </summary>
    string EventCommaList => "";

    /// <summary>
    /// `FileTop` text will appear at the top of the file. Use for comments, copyright notices, code...
    /// </summary>
    string FileTop => "";

    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/161
    /// </summary>
    string TriggerMap => "";
}

public class DummyIRenderConfig : IRenderConfig
{

}
