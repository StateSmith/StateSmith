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

    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .c file.
    /// </summary>
    public string CFileTop = "";

    public string CFileIncludes = "";

    public string CFileBottom = "";

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

        CFileBottom = Process(config.CFileBottom);
        CFileIncludes = Process(config.CFileIncludes);
        CFileTop = Process(config.CFileTop);

        NameSpace = Process(config.NameSpace);

        BaseClassCode = Process(config.BaseClassCode);

        ClassCode = Process(config.ClassCode);
        HFileExtension = config.HFileExtension.Trim();
        IncludeGuardLabel = config.IncludeGuardLabel.Trim();
    }
}
