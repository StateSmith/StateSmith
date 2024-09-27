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
    /// Can be changed to ".hh" (or whatever) if you like.
    /// </summary>
    public string HFileExtension = ".hpp";

    // to declare a namespace, use file top and bottom instead (for now)
    public string NameSpace = "";

    /// <summary>
    /// Use to specify base class(es) for generated state machine class.
    /// Ex: `public MyBaseClass, public MyOtherBaseClass`.
    /// This syntax will be come the "base-clause" https://en.cppreference.com/w/cpp/language/derived_class
    /// </summary>
    public string BaseClassCode = "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    public string ClassCode = "";

    /// <summary>
    /// Will replace `{enumName}` with name of enumeration. Use like this:
    /// <code>
    /// typedef enum __attribute__((packed)) {enumName}
    /// </code>
    /// https://github.com/StateSmith/StateSmith/issues/185
    /// </summary>
    public string EnumDeclarer = "";

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

        NameSpace = Process(config.NameSpace);

        ClassCode = Process(config.ClassCode);
        EnumDeclarer = Process(config.EnumDeclarer);

        HFileExtension = config.HFileExtension.Trim();
        IncludeGuardLabel = config.IncludeGuardLabel.Trim();
    }
}
