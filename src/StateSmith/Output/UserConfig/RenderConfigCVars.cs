#nullable enable

namespace StateSmith.Output.UserConfig;

/// <summary>
/// NOTE! Field name used with reflection for toml parsing.
/// </summary>
public class RenderConfigCVars
{
    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .h file.
    /// </summary>
    public string HFileTop = "";

    public string HFileIncludes = "";

    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .c file.
    /// </summary>
    public string CFileTop = "";

    public string CFileIncludes = "";

    /// <summary>
    /// Can be changed to ".cpp" (or whatever) to support C++ until idiomatic C++ support is added.
    /// </summary>
    public string CFileExtension = ".c";

    /// <summary>
    /// Can be changed to ".hpp" (or whatever) if you like.
    /// </summary>
    public string HFileExtension = ".h";

    /// <summary>
    /// Will replace `{enumName}` with name of enumeration. Use like this:
    /// <code>
    /// typedef enum __attribute__((packed)) {enumName}
    /// </code>
    /// https://github.com/StateSmith/StateSmith/issues/185
    /// </summary>
    public string CEnumDeclarer = "";

    public void SetFrom(IRenderConfigC config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        HFileTop = Process(config.HFileTop);
        HFileIncludes = Process(config.HFileIncludes);

        CFileTop = Process(config.CFileTop);
        CFileIncludes = Process(config.CFileIncludes);

        CFileExtension = Process(config.CFileExtension);
        HFileExtension = Process(config.HFileExtension);

        CEnumDeclarer = Process(config.CEnumDeclarer);
    }
}
