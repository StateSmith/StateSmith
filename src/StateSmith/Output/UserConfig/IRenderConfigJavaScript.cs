namespace StateSmith.Output.UserConfig;

public interface IRenderConfigJavaScript : IRenderConfig
{
    /// <summary>
    /// Use to add custom code to generated state machine class, although <seealso cref="ExtendsSuperClass"/>
    /// may suit your needs better.
    /// </summary>
    string ClassCode => "";

    string ExtendsSuperClass => "";

    bool UseExportOnClass => false;

    /// <summary>
    /// Set to "#" if you want to support new js versions.
    /// Set to "_" if you want to support old js versions.
    /// </summary>
    string PrivatePrefix => "#";
}
