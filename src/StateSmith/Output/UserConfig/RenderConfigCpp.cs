#nullable enable

namespace StateSmith.Output.UserConfig;

/// <summary>
/// NOTE! Field name used with reflection for toml parsing.
/// </summary>
public class RenderConfigCppVars
{
    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .h file.
    /// </summary>
    string HFileTop = "";

    /// <summary>
    /// If blank (default), `#pragma once` will be used. Otherwise, this will be used as the include guard.<br/>
    /// Supports `{FILENAME}` and `{fileName}`.<br/>
    /// https://github.com/StateSmith/StateSmith/issues/112
    /// </summary>
    string IncludeGuardLabel = "";

    string HFileTopPostIncludeGuard = "";

    string HFileIncludes = "";

    string HFileBottomPreIncludeGuard = "";

    string HFileBottom = "";

    ///// <summary>
    ///// Whatever this property returns will be placed at the top of the rendered .c file.
    ///// </summary>
    //string CFileTop = "";

    //string CFileIncludes = "";

    //string CFileBottom = "";

    ///// <summary>
    ///// Can be changed to ".cpp" (or whatever) to support C++ until idiomatic C++ support is added.
    ///// </summary>
    //string CFileExtension = ".c";

    /// <summary>
    /// Can be changed to ".hpp" (or whatever) if you like.
    /// </summary>
    string HFileExtension = ".h";

    // to declare a namespace, use file top and bottom instead (for now)
    //string NameSpace = "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode = "";

    /// <summary>
    /// Will replace `{enumName}` with name of enumeration. Use like this:
    /// <code>
    /// typedef enum __attribute__((packed)) {enumName}
    /// </code>
    /// https://github.com/StateSmith/StateSmith/issues/185
    /// </summary>
    string EnumDeclarer = "";

    public void SetFrom(IRenderConfigCpp config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
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

        ClassCode = Process(config.ClassCode);
        EnumDeclarer = Process(config.EnumDeclarer);

        HFileExtension = config.HFileExtension.Trim();
        IncludeGuardLabel = config.IncludeGuardLabel.Trim();
    }
}
