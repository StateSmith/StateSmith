namespace StateSmith.Output.UserConfig;

public interface IRenderConfigCSharp : IRenderConfig
{
    string NameSpace => "";
    string Usings => "";

    /// <summary>
    /// Use to add custom code to generated state machine class, although <seealso cref="UsePartialClass"/>
    /// may suit your needs better.
    /// </summary>
    string ClassCode => "";

    bool UseNullable => true;

    /// <summary>
    /// Output state machine code will be a partial class. Very useful so you can define other part of 
    /// partial class and easily add functionality to the state machine(fields, methods...).
    /// </summary>
    bool UsePartialClass => true;

    /// <summary>
    /// If non-blank, ":" + <see cref="BaseList"/> will appended after state machine class name to
    /// allow you to have the state machine implement an interface or have a base class.
    /// </summary>
    string BaseList => "";
}
