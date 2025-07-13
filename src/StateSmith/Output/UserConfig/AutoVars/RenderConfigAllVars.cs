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
    }

    public void SetFrom(IRenderConfig iRenderConfig, bool autoDeIndentAndTrimRenderConfigItems)
    {
        Base.SetFrom(iRenderConfig, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigC ircc)
            C.SetFrom(ircc, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigCpp irccpp)
            Cpp.SetFrom(irccpp, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigCSharp irccs)
            CSharp.SetFrom(irccs, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigJavaScript rcjs)
            JavaScript.SetFrom(rcjs, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigTypeScript ts)
            TypeScript.SetFrom(ts, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigJava rcj)
            Java.SetFrom(rcj, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigPython rcp)
            Python.SetFrom(rcp, autoDeIndentAndTrimRenderConfigItems);
    }
}
