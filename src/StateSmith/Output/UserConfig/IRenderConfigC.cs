namespace StateSmith.Output.UserConfig;

public interface IRenderConfigC : IRenderConfig
{
    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .h file.
    /// </summary>
    string HFileTop => "";

    string HFileIncludes => "";
    string CFileIncludes => "";

    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .c file.
    /// </summary>
    string CFileTop => "";

    /// <summary>
    /// Can be changed to ".cpp" (or whatever) to support C++ until idiomatic C++ support is added.
    /// </summary>
    string CFileExtension => ".c";

    /// <summary>
    /// Can be changed to ".hpp" (or whatever) if you like.
    /// </summary>
    string HFileExtension => ".h";

    /// <summary>
    /// Will replace `{enumName}` with name of enumeration. Use like this:
    /// <code>
    /// typedef enum __attribute__((packed)) {enumName}
    /// </code>
    /// https://github.com/StateSmith/StateSmith/issues/185
    /// </summary>
    string CEnumDeclarer => "";
}
