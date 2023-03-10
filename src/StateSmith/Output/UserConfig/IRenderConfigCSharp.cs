namespace StateSmith.Output.UserConfig;

public interface IRenderConfigCSharp : IRenderConfig
{
    string NameSpace => "";
    string Usings => "";

    /// <summary>
    /// Use to add custom code to generated state machine class, although <seealso cref="UsePartialClass"/> this
    /// may suit your needs better.
    /// </summary>
    string ClassCode => "";

    bool UseNullable => true;

    /// <summary>
    /// Output state machine code will be a partial class. Very useful so you can define other part of 
    /// partial class and easily add functionality to the state machine(fields, methods...).
    /// </summary>
    bool UsePartialClass => true;
}
