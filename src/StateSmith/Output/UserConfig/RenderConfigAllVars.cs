#nullable enable

namespace StateSmith.Output.UserConfig;

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
    public readonly RenderConfigJavaVars Java;

    public RenderConfigAllVars()
    {
        Base = new RenderConfigBaseVars();
        C = new RenderConfigCVars();
        CSharp = new RenderConfigCSharpVars();
        JavaScript = new RenderConfigJavaScriptVars();
        Java = new RenderConfigJavaVars();
    }
}
