#nullable enable

using StateSmith;

namespace StateSmith.Output.UserConfig.AutoVars;

public class RenderConfigAllVars
{
    public readonly RenderConfigBaseVars Base;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigCVars C;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigCSharpVars CSharp;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigJavaScriptVars JavaScript;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigTypeScriptVars TypeScript;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigJavaVars Java;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigCppVars Cpp;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigPythonVars Python;

    /// <summary>
    /// NOTE! Field name used with reflection for toml parsing.
    /// </summary>
    public readonly RenderConfigBerryVars Berry;

    public RenderConfigAllVars()
    {
        Base = new RenderConfigBaseVars();
        C = new RenderConfigCVars();
        CSharp = new RenderConfigCSharpVars();
        JavaScript = new RenderConfigJavaScriptVars();
        TypeScript = new RenderConfigTypeScriptVars();
        Java = new RenderConfigJavaVars();
        Python = new RenderConfigPythonVars();
        Cpp = new RenderConfigCppVars();
        Berry = new RenderConfigBerryVars();
    }
}
