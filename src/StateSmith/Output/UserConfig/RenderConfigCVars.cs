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

    /// <summary>
    /// If blank (default), `#pragma once` will be used. Otherwise, this will be used as the include guard.<br/>
    /// Supports `{FILENAME}` and `{fileName}`.<br/>
    /// https://github.com/StateSmith/StateSmith/issues/112
    /// </summary>
    public string IncludeGuardLabel = "";

    public string HFileTopPostIncludeGuard = "";

    public string HFileIncludes = "";

    public string HFileBottomPreIncludeGuard = "";

    public string HFileBottom = "";

    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .c file.
    /// </summary>
    public string CFileTop = "";

    public string CFileIncludes = "";

    public string CFileBottom = "";

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

    /// <summary>
    /// Will use &lt;stdbool.h&gt; and bool type instead of int for boolean
    /// </summary>
    public bool UseStdBool = true;

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
        HFileTopPostIncludeGuard = Process(config.HFileTopPostIncludeGuard);
        HFileIncludes = Process(config.HFileIncludes);
        HFileBottomPreIncludeGuard = Process(config.HFileBottomPreIncludeGuard);
        HFileBottom = Process(config.HFileBottom);

        CFileTop = Process(config.CFileTop);
        CFileIncludes = Process(config.CFileIncludes);
        CFileBottom = Process(config.CFileBottom);

        CFileExtension = Process(config.CFileExtension);
        HFileExtension = Process(config.HFileExtension);

        CEnumDeclarer = Process(config.CEnumDeclarer);

        UseStdBool = config.UseStdBool;
        IncludeGuardLabel = config.IncludeGuardLabel.Trim();
    }
}
