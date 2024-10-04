#nullable enable

namespace StateSmith.Output.UserConfig;

public interface IRenderConfigCpp : IRenderConfig
{
    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .h file.
    /// </summary>
    string HFileTop => "";

    /// <summary>
    /// If blank (default), `#pragma once` will be used. Otherwise, this will be used as the include guard.<br/>
    /// Supports `{FILENAME}` and `{fileName}`.<br/>
    /// https://github.com/StateSmith/StateSmith/issues/112
    /// </summary>
    string IncludeGuardLabel => "";

    string HFileTopPostIncludeGuard => "";

    string HFileIncludes => "";

    string HFileBottomPreIncludeGuard => "";

    string HFileBottom => "";

    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .c file.
    /// </summary>
    string CFileTop => "";

    string CFileIncludes => "";

    string CFileBottom => "";

    /// <summary>
    /// Can be changed to ".cpp" (or whatever) to support C++ until idiomatic C++ support is added.
    /// </summary>
    string CFileExtension => ".cpp";

    /// <summary>
    /// Can be changed to ".hpp" (or whatever) if you like.
    /// </summary>
    string HFileExtension => ".hpp";

    string NameSpace => "";

    /// <summary>
    /// Use to specify base class(es) for generated state machine class.
    /// Ex: `public MyBaseClass, public MyOtherBaseClass`.
    /// This syntax will be come the "base-clause" https://en.cppreference.com/w/cpp/language/derived_class
    /// </summary>
    string BaseClassCode => "";

    /// <summary>
    /// Use to add custom code to generated state machine class.
    /// </summary>
    string ClassCode => "";
}
